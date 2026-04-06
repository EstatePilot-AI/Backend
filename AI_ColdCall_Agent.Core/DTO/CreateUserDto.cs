using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO;

public class CreateUserDto
{
	[Required(ErrorMessage = "Person Name can't be blank")]
	public string PersonName { get; set; } = string.Empty;


	[Required(ErrorMessage = "Email can't be blank")]
	[EmailAddress(ErrorMessage = "Email should be in a proper email address format")]
	public string Email { get; set; } = string.Empty;

	[Required(ErrorMessage = "Password can't be blank")]
	public string Password { get; set; } = string.Empty;

	[Required(ErrorMessage = "Phone number can't be blank")]
	[RegularExpression(@"^\+201[0125][0-9]{8}$", ErrorMessage = "Phone number must start with +2 followed by a valid Egyptian mobile number")]
	public string PhoneNumber { get; set; } = string.Empty;

	[Required(ErrorMessage = "Role can't be blank"), MaxLength(30)]
	public string Role { get; set; }
}
