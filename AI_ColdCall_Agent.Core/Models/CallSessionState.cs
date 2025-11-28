using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models;

public class CallSessionState
{
	[Key]
	public int Id { get; set; }

	[Required, MaxLength(100)]
	public string Name { get; set; }
}
