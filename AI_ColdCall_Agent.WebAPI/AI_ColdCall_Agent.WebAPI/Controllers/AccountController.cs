using DTO;
using Identity;
using Interfaces;
using IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly EmailSender _emailSender;
	private readonly IJWTService _jwtService;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly RoleManager<ApplicationRole> _roleManager;
	private readonly IConfiguration _config;

	//inject All services into constructor
	public AccountController(IUnitOfWork unitOfWork, EmailSender emailSender, IJWTService jwtService, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IConfiguration config)
	{
		_unitOfWork = unitOfWork;
		_emailSender = emailSender;
		_jwtService = jwtService;
		_userManager = userManager;
		_signInManager = signInManager;
		_roleManager = roleManager;
		_config = config;
	}

	[Authorize(Roles ="superadmin")]
	[HttpPost("CreateNewUser")]
	public async Task<IActionResult> CreateNewUser(CreateUserDto createUserDto)
	{
		if (ModelState.IsValid)
		{
			var user =await _userManager.FindByEmailAsync(createUserDto.Email);

			if(user == null)
			{
				var newUser = new ApplicationUser
				{
					Name = createUserDto.PersonName,
					Email = createUserDto.Email,
					UserName = createUserDto.Email,
					PhoneNumber = createUserDto.PhoneNumber
				};

				var result = await _userManager.CreateAsync(newUser, createUserDto.Password);

				if (result.Succeeded)
				{
					if(await _roleManager.FindByNameAsync(createUserDto.Role.ToLower()) is null)
					{
						var role = new ApplicationRole
						{
							Name = createUserDto.Role.ToLower()
						};

						await _roleManager.CreateAsync(role);
					}
					await _userManager.AddToRoleAsync(newUser, createUserDto.Role.ToLower());

					var subject = "Account Created";
					var htmlMessage= HTMLMessages.ConfirmationEmailAboutCreatedAccount(newUser.Name, newUser.Email, createUserDto.Password);

					 _emailSender.SendEmail(subject, newUser.Email, htmlMessage);

					return Ok("User account is created successfully.");
				}
				else
				{
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError("Register", error.Description);
					}
					return BadRequest(ModelState);
				}
			}
			return BadRequest("This email is already registered.");
		}
		return BadRequest(ModelState);
	}

	[HttpPost("Login")]
	public async Task<IActionResult> Login(LoginDto loginDto)
	{
		if (ModelState.IsValid)
		{
			var user = await _userManager.FindByEmailAsync(loginDto.Email);

			if (user != null)
			{
				var result = await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, isPersistent: false, lockoutOnFailure: false);

				if (result.Succeeded)
				{
					await _signInManager.SignInAsync(user, isPersistent: false);

					var token = await _jwtService.CreateJwtToken(user);
					
					return Ok(token);
				}
			}
			return BadRequest("Invalid Email or Password");
		}
		return BadRequest(ModelState);
	}

	[HttpPost("ForgetPassword")]
	public async Task<IActionResult> ForgetPassword(ForgetPasswordDto forgetPasswordDto)
	{
		if (ModelState.IsValid)
		{
			var user = await _userManager.FindByEmailAsync(forgetPasswordDto.Email);

			if (user != null)
			{
				var token = await _userManager.GeneratePasswordResetTokenAsync(user);

				var htmlMessage = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Reset Code</title>
    <style>
        body {{ margin: 0; padding: 0; font-family: Helvetica, Arial, sans-serif; background-color: #ffffff; color: #333333; }}
        .code-text {{
            font-family: 'Courier New', monospace;
            font-size: 32px;
            font-weight: bold;
            letter-spacing: 5px;
            color: #000000;
            background-color: #f4f4f4;
            padding: 10px 20px;
            border-radius: 4px;
            display: inline-block;
            margin: 15px 0;
        }}
    </style>
</head>
<body style=""margin: 0; padding: 20px;"">

    <div style=""max-width: 400px; margin: 0 auto; text-align: center;"">
        
        <p style=""font-size: 14px; font-weight: bold; color: #888; text-transform: uppercase; margin-bottom: 20px;"">
            Cold Call Agent Company
        </p>

        <h2 style=""font-size: 18px; margin: 0 0 10px 0;"">Password Reset</h2>
        <p style=""font-size: 14px; color: #666; margin: 0;"">Use this code to verify your identity:</p>

        <div class=""code-text"">
            {token}
        </div>

        <p style=""font-size: 12px; color: #999; margin-top: 10px;"">
            Valid for 15 minutes.
        </p>

    </div>

</body>
</html>";

				_emailSender.SendEmail("Password Reset Code", forgetPasswordDto.Email, htmlMessage);

				return Ok(new { message = "Password reset code sent." });
			}
			return NotFound("User with this email is not found.");
		}
		return BadRequest(ModelState);
	}

	[HttpPost("ResetPassword")]
	public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
	{
		if (ModelState.IsValid)
		{
			var user =await _userManager.FindByEmailAsync(resetPasswordDto.Email);
			if(user != null)
			{
				var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

				if (result.Succeeded)
				{
					return Ok(new { message = "Password has been reset successfully." });
				}
				return BadRequest(result.Errors);
			}
			return NotFound("User with this email is not found.");
		}
		return BadRequest(ModelState);
	}

	[Authorize]
	[HttpPut("UpdateProfile")]
	public async Task<IActionResult> UpdateProfile(UpdateProfileDto updateProfileDto)
	{
		if (ModelState.IsValid)
		{
			var user = await _userManager.GetUserAsync(User);

			if (user != null)
			{
				user.Name= updateProfileDto.Name;
				user.PhoneNumber= updateProfileDto.PhoneNumber;

				var result =await _userManager.UpdateAsync(user);

				if (result.Succeeded)
				{
					await _signInManager.RefreshSignInAsync(user);

					return Ok("Profile updated successfully.");
				}

				foreach(var error in result.Errors)
				{
					ModelState.AddModelError("UpdateProfile", error.Description);
				}
				return BadRequest(ModelState);
			}
			return NotFound("User not found.");
		}
		return BadRequest(ModelState);
	}

	[Authorize]
	[HttpPut("ChangePassword")]
	public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
	{
		if (ModelState.IsValid)
		{
			var user =await _userManager.GetUserAsync(User);
			if(user != null)
			{
				var result =await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

				if (result.Succeeded)
				{
					await _signInManager.RefreshSignInAsync(user);
					return Ok("Password changed successfully.");
				}
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError("ChangePassword", error.Description);
				}
				return BadRequest(ModelState);
			}
			return NotFound("User not found.");
		}
		return BadRequest(ModelState);
	}

	[Authorize(Roles ="superadmin")]
	[HttpGet("GetAllUsers")]
	public async Task<IActionResult> GetAllUsers()
	{
		var users=await _userManager.Users.ToListAsync();
		var usersListDto=new List<UserDto>();

		if (users != null)
		{
			foreach(var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);

				usersListDto.Add(new UserDto
				{
					Id = user.Id,
					Name = user.Name,
					Email = user.Email!,
					PhoneNumber = user.PhoneNumber!,
					Role = roles.FirstOrDefault()!
				});
			}
			usersListDto=usersListDto.Where(u=>u.Role!="superadmin").ToList();
		}
		return Ok(usersListDto);
	}


	[Authorize(Roles = "superadmin")]
	[HttpGet("GetUserById/{id}")]
	public async Task<IActionResult> GetUserById(Guid id)
	{
		var user = await _userManager.FindByIdAsync(id.ToString());

		if (user == null)
		{
			return NotFound("User not found.");
		}

		var userDto = new UserDto
		{
			Id = user.Id,
			Name = user.Name,
			Email = user.Email!,
			PhoneNumber = user.PhoneNumber!,
			Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()!
		};

		return Ok(userDto);
	}

	[Authorize]
	[HttpGet("GetMyProfile")]
	public async Task<IActionResult> GetMyProfile()
	{
		var user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return NotFound("User not found.");
		}

		var userDto = new UserDto
		{
			Id = user.Id,
			Name = user.Name,
			Email = user.Email!,
			PhoneNumber = user.PhoneNumber!,
			Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()!
		};

		return Ok(userDto);
	}

	[HttpPost("Logout")]
	public async Task<IActionResult> Logout()
	{
		await _signInManager.SignOutAsync();

		return Ok(new { message = "Logged out successfully." });
	}

}
