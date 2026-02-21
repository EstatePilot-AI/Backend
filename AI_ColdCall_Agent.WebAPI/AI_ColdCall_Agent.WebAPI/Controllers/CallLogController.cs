using DTO;
using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class CallLogController : ControllerBase
{
	private readonly IUnitOfWork _unitOfWork;

	public CallLogController(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	[HttpGet("GetAllCallLogs")]
	public async Task<IActionResult> GetAllCallLogs()
	{
		var callLogs = await _unitOfWork.CallLogs.FindAllWithIncludeAsync(new string[] { "Contact", "CallOutcome", "SubjectTypeCall", "CallSessionState" });

		if(callLogs == null)
		{
			return NotFound(new
			{
				error = "There is no call Logs in the system"
			});
		}

		var callLogsDto = callLogs.Select(cl => new
		{
			CallId = cl.CallId,
			BuyerName = cl.Contact.Name,
			CallOutcome = cl.CallOutcome.Name,
			CallType = cl.SubjectTypeCall.Name,
			CallSessionState = cl.CallSessionState.Name,
			Duration=cl.Duration,
			TimeStamp = GetTimeAgo(cl.Timestamp)
		});

		return Ok(callLogsDto);
	}

	[HttpGet("GetCallLogById/{id:int}")]
	public async Task<IActionResult> GetCallLogById(int id)
	{
		if(id <= 0)
		{
			return BadRequest(new
			{
				error = "Id must be more than 0"
			});
		}

		var callLog = _unitOfWork.CallLogs.FindOneItem(cl => cl.CallId == id, new string[] { "Contact", "CallOutcome", "SubjectTypeCall", "CallSessionState" });

		if (callLog == null)
		{
			return NotFound(new
			{
				error = $"Call Log with Id: {id} not found"
			});
		}

		var callLogDto = new CallLogDto
		{
			CallId = callLog.CallId,
			BuyerName = callLog.Contact.Name,
			CallOutcome = callLog.CallOutcome.Name,
			CallType = callLog.SubjectTypeCall.Name,
			CallSessionState = callLog.CallSessionState.Name,
			Summary = callLog.Transcript,
			Duration = callLog.Duration,
			RetryCount = callLog.RetryCount,
			TimeStamp = callLog.Timestamp
		};

		return Ok(callLogDto);
	}
	//handle the time like just now, 1 minute ago and so on
	private string GetTimeAgo(DateTime date)
	{
		var timeDifference = DateTime.UtcNow - date;

		if (timeDifference.TotalSeconds < 60)
		{
			return "Just now";
		}

		// Less than 1 hour ago
		if (timeDifference.TotalMinutes < 60)
		{
			return $"{(int)timeDifference.TotalMinutes}m ago";
		}

		// Less than 24 hours ago
		if (timeDifference.TotalHours < 24)
		{
			return $"{(int)timeDifference.TotalHours}h ago";
		}

		// Less than 7 days ago (optional extension)
		if (timeDifference.TotalDays < 7)
		{
			return $"{(int)timeDifference.TotalDays}d ago";
		}

		// Fallback for older dates (e.g., "Oct 24")
		return date.ToString("MMM dd");
	}
}

