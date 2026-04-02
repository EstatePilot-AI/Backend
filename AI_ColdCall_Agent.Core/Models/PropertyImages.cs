using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models;

public class PropertyImages
{
	public int Id { get; set; }

	[ForeignKey("Property")]
	public int PropertyId { get; set; }
	public Property Property { get; set; }
	public string ImageURL { get; set; }
}
