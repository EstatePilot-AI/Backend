using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO;

public class UpdateProfileDto
{
	[Required(ErrorMessage = "Person Name can't be blank")]
	public string Name {  get; set; }

	[Required(ErrorMessage = "Phone number can't be blank")]
	[RegularExpression(@"^(\+20)?(010|011|012|015)[0-9]{8}$", ErrorMessage = "Phone number must be a valid Egyptian mobile number")]
	public string PhoneNumber { get; set; }
}
