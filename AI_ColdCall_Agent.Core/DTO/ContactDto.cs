using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DTO;

public class ContactDto
{
	[Required, MaxLength(100)]
	public string Name { get; set; }

	[Required]
	[RegularExpression(@"^\+201[0125][0-9]{8}$", ErrorMessage = "Phone number must start with +2 followed by a valid Egyptian mobile number")] public string Phone { get; set; }
	[Required]
	[EmailAddress(ErrorMessage = "Email should be in a proper email address format")]
	public string Email { get; set; }
}
