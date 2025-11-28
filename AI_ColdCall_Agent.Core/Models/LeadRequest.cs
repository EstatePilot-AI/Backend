using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models;

public class LeadRequest
{
	[Key]
	public int RequestId { get; set; }

	[Required, ForeignKey("Contact")]
	public int BuyerContactId { get; set; }
	public Contact Contact { get; set; }

	[Required, ForeignKey("Property")]
	public int PropertyId { get; set; }
	public Property Property { get; set; }

	[Required, ForeignKey("LeadRequestStatus")]
	public int LeadRequestStatusId { get; set; }
	public LeadRequestStatus LeadRequestStatus { get; set; }
}
