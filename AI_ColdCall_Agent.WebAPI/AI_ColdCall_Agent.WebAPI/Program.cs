using DatabaseContext;
using Identity;
using Interfaces;
using IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repositories;
using Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
	Args = args,
	WebRootPath = "wwwroot"
});

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger with JWT Authentication
builder.Services.AddSwaggerGen(swagger =>
{
	swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
	{
		Name = "Authorization",
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Enter 'Bearer' [space] and then your valid token.\r\nExample: Bearer eyJhbGci...",
	});

	swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] { }
		}
	});
});


// Configure CORS to allow requests from any origin (you can customize this for production)
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.SetIsOriginAllowed(origin => true)
			  .AllowAnyMethod()
			  .AllowAnyHeader()
			  .AllowCredentials(); //for SignalR
	});
});


var con = builder.Configuration.GetConnectionString("con");
builder.Services.AddDbContext<AI_ColdCall_Agent_DbContext>(options => options.UseNpgsql(con));


//Inject Services
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();  // inject BackgroundTaskQueue as a singleton for queuing
builder.Services.AddHostedService<AIOutboundCallWorker>();
builder.Services.AddHostedService<AISellerCallWorker>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<EmailSender>();   // inject EmailSender service
builder.Services.AddTransient<IJWTService, JWTService>();  //inject JWTService

builder.Services.AddScoped<DashboardAnalyticsService>(); 
builder.Services.AddHttpClient();

builder.Services.AddSignalR(options => {
	options.EnableDetailedErrors = true; // This will tell you EXACTLY why it failed in the console
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
	options.Password.RequiredLength = 8;
	options.Password.RequireNonAlphanumeric = false;
	options.Password.RequireUppercase = false;
	options.Password.RequireLowercase = false;
	options.Password.RequireDigit = false;
	// Configure password reset to use your custom 6-digit provider
	options.Tokens.PasswordResetTokenProvider = "EmailOTP";
})
.AddEntityFrameworkStores<AI_ColdCall_Agent_DbContext>()
.AddDefaultTokenProviders()
.AddUserStore<UserStore<ApplicationUser, ApplicationRole, AI_ColdCall_Agent_DbContext, Guid>>()
.AddRoleStore<RoleStore<ApplicationRole, AI_ColdCall_Agent_DbContext, Guid>>()
.AddTokenProvider<EmailOtpTokenProvider<ApplicationUser>>("EmailOTP"); //register the custom token provider

//JWT
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
	{
		ValidateAudience = true,
		ValidAudience = builder.Configuration["JWT:Audience"],
		ValidateIssuer = true,
		ValidIssuer = builder.Configuration["JWT:Issuer"],
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecritKey"]))
	};

	options.Events = new JwtBearerEvents
	{
		OnMessageReceived = context =>
		{
			var accessToken = context.Request.Query["access_token"];

			// If the request is for our hub...
			var path = context.HttpContext.Request.Path;
			if (!string.IsNullOrEmpty(accessToken) &&
				path.StartsWithSegments("/dashboardHub"))
			{
				// Read the token out of the query string
				context.Token = accessToken;
			}
			return Task.CompletedTask;
		}
	};
});

builder.Services.AddAuthorization();

// Handle Reference Loops in JSON Serialization
builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});


var app = builder.Build();

app.UseStaticFiles(); //for wwwroot

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

//if (!app.Environment.IsDevelopment())
//{
//	app.UseSwagger();
//	app.UseSwaggerUI(c =>
//	{
//		c.SwaggerEndpoint("/swagger/v1/swagger.json", "my api v1");
//		c.RoutePrefix = string.Empty; // set to empty string to serve at root
//	});
//}

app.UseHttpsRedirection();


//Enable CORS
app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Map the hub so the frontend can connect
app.MapHub<DashboardHub>("/dashboardHub");
app.MapControllers();

app.Run();