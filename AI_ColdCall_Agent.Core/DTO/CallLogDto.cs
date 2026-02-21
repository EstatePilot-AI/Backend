using System;
using System.Collections.Generic;
using System.Text;

namespace DTO;

public class CallLogDto
{
	public int CallId { get; set; }
	public string BuyerName { get; set; }
	public string CallOutcome { get; set; }
	public string CallType { get; set; }
	public string CallSessionState { get; set; }
	public string Summary { get; set; }
	public int Duration { get; set; }
	public int RetryCount { get; set; }
	public DateTime TimeStamp { get; set; }
}
