using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO;

public class ResetPasswordDto
{
	[Required(ErrorMessage = "Email can't be blank")]
	[EmailAddress(ErrorMessage = "Email should be in a proper email address format")]
	public string Email { get; set; }
	public string Token { get; set; }

	[Required(ErrorMessage = "Password can't be blank")]
	public string NewPassword { get; set; }
}
