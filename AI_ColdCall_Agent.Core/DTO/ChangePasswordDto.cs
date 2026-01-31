using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO;

public class ChangePasswordDto
{
	[Required(ErrorMessage = "Password can't be blank"), MinLength(8)]
	public string CurrentPassword { get; set; }

	[Required(ErrorMessage = "Password can't be blank"), MinLength(8)]
	public string NewPassword { get; set; }
}
