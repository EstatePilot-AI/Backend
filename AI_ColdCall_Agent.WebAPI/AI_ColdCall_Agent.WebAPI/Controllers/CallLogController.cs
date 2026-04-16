using AI_ColdCall_Agent.Core.DTO;
using DTO;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Linq.Expressions;

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
	public async Task<IActionResult> GetAllCallLogs([FromQuery] CallLogFilterDto filter)
	{
		Expression<Func<Models.CallLog, bool>> predicate = cl =>
			(!filter.CallOutcomeId.HasValue || cl.CallOutcomeId == filter.CallOutcomeId) &&
			(!filter.CallSessionStateId.HasValue || cl.CallSessionStateId == filter.CallSessionStateId) &&
			(!filter.FromDate.HasValue || cl.Timestamp >= filter.FromDate.Value) &&
			(!filter.ToDate.HasValue || cl.Timestamp <= filter.ToDate.Value.AddDays(1)) &&
			(string.IsNullOrWhiteSpace(filter.SearchTerm) ||
			 cl.ContactName.Contains(filter.SearchTerm));

		var includes = new[] { "Contact", "CallOutcome", "SubjectTypeCall", "CallSessionState" };

		var (items, totalCount) = await _unitOfWork.CallLogs.GetPaginatedAsync(
			predicate,
			includes,
			q => q.OrderByDescending(cl => cl.Timestamp),
			filter.PageNumber,
			filter.PageSize);

		var data = items.Select(cl => new
		{
			CallId = cl.CallId,
			BuyerName = cl.ContactName,
			CallOutcome = cl.CallOutcome.Name,
			CallType = cl.SubjectTypeCall.Name,
			CallSessionState = cl.CallSessionState.Name,
			Duration = cl.Duration,
			TimeStamp = GetTimeAgo(cl.Timestamp)
		}).ToList();

		var result = new PaginatedResult<object>
		{
			Data = data,
			TotalCount = totalCount,
			PageNumber = filter.PageNumber,
			PageSize = filter.PageSize,
		};

		return Ok(result);
	}

	[HttpGet("GetCallOutcome")]
	public async Task<IActionResult> GetCallOutcome()
	{

		var statuses = await _unitOfWork.CallOutcomes.GetAllAsync();

		var response = statuses.Select(s => new
		{
			Id = s.Id,
			Name = s.Name
		}).ToList();

		return Ok(response);
	}

	[HttpGet("GetCallLogById/{id:int}")]
	public async Task<IActionResult> GetCallLogById(int id)
	{
		if (id <= 0)
		{
			return BadRequest(new
			{
				error = "Please provide a valid call log ID (must be a positive number)."
			});
		}

		var callLog = _unitOfWork.CallLogs.FindOneItem(cl => cl.CallId == id, new string[] { "Contact", "CallOutcome", "SubjectTypeCall", "CallSessionState" });

		if (callLog == null)
		{
			return NotFound(new
			{
				error = $"We couldn't find a call log with ID {id}. It may have been deleted."
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

	private string GetTimeAgo(DateTime date)
	{
		var timeDifference = DateTime.UtcNow - date;

		if (timeDifference.TotalSeconds < 60)
		{
			return "Just now";
		}

		if (timeDifference.TotalMinutes < 60)
		{
			return $"{(int)timeDifference.TotalMinutes}m ago";
		}

		if (timeDifference.TotalHours < 24)
		{
			return $"{(int)timeDifference.TotalHours}h ago";
		}

		if (timeDifference.TotalDays < 7)
		{
			return $"{(int)timeDifference.TotalDays}d ago";
		}

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
					message = "There are no call logs to delete at the moment."
				}
			});
		}

		foreach (var callLog in callLogs)
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
