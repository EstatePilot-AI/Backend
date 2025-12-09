using System.Text.Json.Serialization;
using DatabaseContext;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var con = builder.Configuration.GetConnectionString("con");
builder.Services.AddDbContext<AI_ColdCall_Agent_DbContext>(options => options.UseNpgsql(con));


//Inject Services
builder.Services.AddScoped <IUnitOfWork, UnitOfWork>();


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
