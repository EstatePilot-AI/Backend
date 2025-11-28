using Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models;

public class Deal
{
	[Key]
	public int DealId { get; set; }

	[Required, ForeignKey("BuyerContact")]
	public int BuyerContactId { get; set; }
	public Contact BuyerContact { get; set; }

	[Required, ForeignKey("SellerContact")]
	public int SellerContactId { get; set; }
	public Contact SellerContact { get; set; }

	[Required, ForeignKey("Property")]
	public int PropertyId { get; set; }
	public Property Property { get; set; }

	[Required, ForeignKey("DealStatus")]
	public int DealStatusId { get; set; }
	public DealStatus DealStatus { get; set; }

	[Required, ForeignKey("Agent")]
	public Guid AgentId { get; set; }
	public  ApplicationUser Agent { get; set; }

	[Required, Range(0.01, double.MaxValue)]
	public decimal Amount { get; set; }

	[Required]
	public DateTime DealDate { get; set; }
}
