using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models;

public class Property
{
	[Key]
	public int PropertyId { get; set; }

	[Required, ForeignKey("Contact")]
	public int SellerContactId { get; set; }
	public virtual Contact Contact { get; set; }

	[Required, ForeignKey("PropertyType")]
	public int PropertyTypeId { get; set; }
	public virtual PropertyType PropertyType { get; set; }

	[Required, ForeignKey("PropertyStatus")]
	public int PropertyStatusId { get; set; }
	public virtual PropertyStatus PropertyStatus { get; set; }

	[Required, Range(0.01, double.MaxValue)]
	public decimal Price { get; set; }

	[Required, Range(0.01, double.MaxValue)]
	public decimal Area { get; set; }

	[Required, Range(0, int.MaxValue)]
	public int Rooms { get; set; }

	[Required, Range(0, int.MaxValue)]
	public int Bathrooms { get; set; }

	[Required, Range(0.01, double.MaxValue)]
	public decimal DownPayment { get; set; }

	[Required, ForeignKey("PaymentMethod")]
	public int PaymentMethodId { get; set; }
	public PaymentMethod PaymentMethod { get; set; }

	[Required, ForeignKey("FinishingType")]
	public int FinishingTypeId { get; set; }
	public FinishingType FinishingType { get; set; }

	[Required, ForeignKey("ListingType")]
	public int ListingTypeId { get; set; }
	public ListingType ListingType { get; set; }

	public string Description { get; set; }

	public bool Negotiable { get; set; }

	[Required]
	public DateTime CreatedAt {  get; set; }

	// 1-to-1 with Location

	public virtual PropertiesLocation PropertiesLocation { get; set; }

	public ICollection<PropertyImages>? propertyImages { get; set; }

}
