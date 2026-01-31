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
	[RegularExpression(@"^(\+20)?(010|011|012|015)[0-9]{8}$", ErrorMessage = "Phone number must be a valid Egyptian mobile number")]
	public string Phone { get; set; }

	[Required]
	[EmailAddress(ErrorMessage = "Email should be in a proper email address format")]
	public string Email { get; set; }
}
