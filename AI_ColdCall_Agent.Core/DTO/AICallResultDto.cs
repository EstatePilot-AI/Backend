using System;
using System.Collections.Generic;
using System.Text;

namespace DTO;

public class AICallResultDto
{
	public string leadID { get; set; }
	public string ContactName { get; set; }
	public string callId { get; set; }
	public string summary { get; set; }
	public double Duration { get; set; } //in seconds
	public string CallOutcome { get; set; } //Interested, notInterested, noAnswer or busy

	public string? propertyId { get; set; } //optional, only provided if the call outcome is interested

}