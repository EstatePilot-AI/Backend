using DTO;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
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
	public async Task<IActionResult> GetAllCallLogs([FromQuery]int? callOutComeId)
	{
		var callLogs = await _unitOfWork.CallLogs.FindAllWithIncludeAsync(new string[] { "Contact", "CallOutcome", "SubjectTypeCall", "CallSessionState" });

		if (callOutComeId.HasValue)
		{
			callLogs = callLogs.Where(cl => cl.CallOutcomeId == callOutComeId);
		}

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
			BuyerName = cl.ContactName,
			CallOutcome = cl.CallOutcome.Name,
			CallType = cl.SubjectTypeCall.Name,
			CallSessionState = cl.CallSessionState.Name,
			Duration = cl.Duration,
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
			BuyerName = callLog.ContactName,
			CallOutcome = callLog.CallOutcome.Name,
			CallType = callLog.SubjectTypeCall.Name,
			CallSessionState = callLog.CallSessionState.Name,
			CallRecordingId = callLog.CallIDFromAI,
			Summary = callLog.Transcript,
			Duration = callLog.Duration,
			RetryCount = callLog.RetryCount,
			TimeStamp = callLog.Timestamp,
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


	[Authorize(Roles = "superadmin")]
	[HttpDelete("DeleteAllCallLogs")]
	public async Task<IActionResult> DeleteAllCallLogs()
	{
		var callLogs = await _unitOfWork.CallLogs.GetAllAsync();

		if (callLogs == null)
		{
			return NotFound(new
			{
				status = "error",
				error = new
				{
					message = "No call logs found to delete."
				}
			});
		}

		foreach(var callLog in callLogs)
		{
			_unitOfWork.CallLogs.Delete(callLog);
		}

		_unitOfWork.Save();

		return Ok(new
		{
			status = "success",
			message = "All call logs have been deleted successfully."
		});
	}
    [HttpGet("GetCallLogsCountWithDetails")]
    public async Task<IActionResult> GetCallLogsCountWithDetails(int? day, int? month, int? year, string outcome = null)
    {
       
        var allLogs = await _unitOfWork.CallLogs.GetAllWithIncludesAsync(
            c => c.CallOutcome,
            c => c.SubjectTypeCall,
            c => c.CallSessionState
        );

       
        var availableStatuses = allLogs
                                .Where(c => c.CallOutcome != null)
                                .Select(c => c.CallOutcome.Name)
                                .Distinct()
                                .ToList();

        var filteredQuery = allLogs.AsQueryable();

        if (year.HasValue && year > 0)
            filteredQuery = filteredQuery.Where(c => c.Timestamp.Year == year);

        if (month.HasValue && month > 0)
            filteredQuery = filteredQuery.Where(c => c.Timestamp.Month == month);

        if (day.HasValue && day > 0)
            filteredQuery = filteredQuery.Where(c => c.Timestamp.Day == day);

    
        if (!string.IsNullOrWhiteSpace(outcome) && outcome.ToLower() != "all")
        {
            
            filteredQuery = filteredQuery.Where(c => c.CallOutcome != null &&
                                                     c.CallOutcome.Name.Trim().ToLower() == outcome.Trim().ToLower());
        }

     
        var filteredList = filteredQuery.ToList();

        var details = filteredList.Select(cl => new
        {
            cl.CallId,
            cl.ContactName,
            Outcome = cl.CallOutcome?.Name ?? "N/A",
            Type = cl.SubjectTypeCall?.Name ?? "N/A",
            Status = cl.CallSessionState?.Name ?? "N/A",
            cl.Duration,
            ExactTime = cl.Timestamp.ToString("yyyy-MM-dd"),
            
        }).ToList();

        return Ok(new
        {
            AvailableStatuses = availableStatuses,
            TotalCount = details.Count, 
            FilterApplied = new
            {
               
                Status = string.IsNullOrWhiteSpace(outcome) ? "All" : outcome
            },
            Data = details 
        });
    }

}

