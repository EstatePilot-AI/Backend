using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models;

public class ListingType
{
	[Key]
	public int ListingTypeId { get; set; }

	[Required, MaxLength(100)]
	public string Name { get; set; } // "For Sale", "For Rent"
}
