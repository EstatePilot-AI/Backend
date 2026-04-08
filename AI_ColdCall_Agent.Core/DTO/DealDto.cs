using System;
using System.Collections.Generic;
using System.Text;

namespace DTO;

public class DealDto
{
	public int DealId { get; set; }
	public string Buyer { get; set; } = string.Empty;
	public string Seller { get; set; } = string.Empty;
	public string Agent { get; set; } = string.Empty;
	public string MeetingDate { get; set; } = string.Empty;
	public string? MeetingLocation { get; set; }
	public string MeetingStatus { get; set; } = string.Empty;
	public string DealStatus { get; set; } = string.Empty;
	public decimal FinalSaleAmount { get; set; }
	public string DealDate { get; set; } = string.Empty;
}
