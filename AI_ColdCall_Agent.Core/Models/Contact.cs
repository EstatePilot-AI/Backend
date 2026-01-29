using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models;

[Index(nameof(Phone), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class Contact
{
	[Key]
	public int ContactId { get; set; }

	[Required, MaxLength(100)]
	public string Name { get; set; }

	[Required]
	[RegularExpression(@"^(\+20)?(010|011|012|015)[0-9]{8}$", ErrorMessage = "Phone number must be a valid Egyptian mobile number")]
	public string Phone { get; set; }

	[EmailAddress(ErrorMessage = "Email should be in a proper email address format")]
	public string Email { get; set; }

	[ForeignKey("ContactType")]
	public int ContactTypeId { get; set; }
	public ContactType ContactType { get; set; }

	[ForeignKey("ContactStatus")]
	public int ContactStatusId { get; set; }
	public ContactStatus ContactStatus { get; set; }

	// One-to-Many with BuyerPreference
	public List<BuyerReference> BuyerPreferences { get; set; }
}
