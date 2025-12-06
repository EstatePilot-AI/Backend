using Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text;

namespace Models;

public class UserHistory
{
	[Key]
	public int UserHistoryId { get; set; }

	[Required, ForeignKey("User")]
	public Guid UserId { get; set; }
	public ApplicationUser User { get; set; }

	[Required, ForeignKey("Role")]
	public Guid UserRoleId { get; set; }
	public ApplicationRole Role { get; set; }

	[Required]
	public DateTime TimeStamp { get; set; }
}
