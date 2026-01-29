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


	[Required(ErrorMessage = "Phone number can't be blank")]
	[RegularExpression(@"^(\+20)?(010|011|012|015)[0-9]{8}$", ErrorMessage = "Phone number must be a valid Egyptian mobile number")]
	public string PhoneNumber { get; set; } = string.Empty;

	[Required(ErrorMessage = "Role can't be blank"), MaxLength(30)]
	public string Role { get; set; }
}
