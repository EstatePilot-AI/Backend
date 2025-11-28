using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models;

public class BuyerReference
{
	[Key]
	public int Id { get; set; }

	[Required, ForeignKey("Contact")]
	public int ContactId { get; set; }
	public Contact Contact { get; set; }

	[Required, Range(0.01, double.MaxValue)]
	public decimal Budget { get; set; }

	[Required]
	public string PreferredLocations { get; set; }

	[Required, ForeignKey("PropertyType")]
	public int PropertyTypeId { get; set; }
	public PropertyType PropertyType { get; set; }

	[Required]
	public string MinimumRequirements { get; set; }
}
