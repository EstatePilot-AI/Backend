using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;
using System.Text;

namespace Models;

public class CallLog
{
	[Key]
	public int CallId { get; set; }

	[Required, ForeignKey("Contact")]
	public int ContactId { get; set; }
	public Contact Contact { get; set; }

	[Required]
	public string ContactName { get; set; }

	[Required, ForeignKey("CallOutcome")]
	public int CallOutcomeId { get; set; }
	public CallOutcome CallOutcome { get; set; }

	[Required, ForeignKey("SubjectTypeCall")]
	public int SubjectTypeId { get; set; }
	public SubjectTypeCall SubjectTypeCall { get; set; }

	[Required]
	public string Transcript { get; set; }

	[Required]
	public DateTime Timestamp { get; set; }

	[Required, Range(0, int.MaxValue)]
	public int Duration { get; set; }

	[Required, ForeignKey("CallSessionState")]
	public int CallSessionStateId { get; set; }
	public CallSessionState CallSessionState { get; set; }

	[Required, Range(0, int.MaxValue)]
	public int RetryCount { get; set; }

	public string CallIDFromAI { get; set; }
}
