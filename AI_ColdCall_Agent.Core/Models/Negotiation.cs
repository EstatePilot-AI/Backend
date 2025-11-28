using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models;

public class Negotiation
{
	[Key]
	public int Id { get; set; }

	[Required, ForeignKey("Property")]
	public int PropertyId { get; set; }
	public Property Property { get; set; }

	[Required, ForeignKey("BuyerContact")]
	public int BuyerContactId { get; set; }
	public Contact BuyerContact { get; set; }

	[Required, Range(0.01, double.MaxValue)]
	public decimal BuyerOfferPrice { get; set; }

	[Required, ForeignKey("NegotiationStatus")]
	public int NegotiationStatusId { get; set; }
	public NegotiationStatus NegotiationStatus { get; set; }
}
