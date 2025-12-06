using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models;

public class PropertiesLocation
{
	[Key]
	public int LocationId { get; set; }

	[Required, ForeignKey("Property")]
	public int PropertyId { get; set; }
	public virtual Property Property { get; set; }

	[Required, MaxLength(100)]
	public string Country { get; set; }

	[Required, MaxLength(100)]
	public string Governorate { get; set; }

	[Required, MaxLength(100)]
	public string City { get; set; }

	[Required, MaxLength(100)]
	public string District { get; set; }

	[Required, MaxLength(150)]
	public string Street { get; set; }

	[Required, Range(1, int.MaxValue)]
	public int BuildingNumber { get; set; }

	[Required, Range(0, int.MaxValue)]
	public int FloorNumber { get; set; }

	[Required, Range(0, int.MaxValue)]
	public int ApartmentNumber { get; set; }
}
