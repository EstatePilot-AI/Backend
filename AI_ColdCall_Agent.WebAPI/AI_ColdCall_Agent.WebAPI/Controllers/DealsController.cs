using AI_ColdCall_Agent.Core.DTO;
using DTO;
using Identity;
using Interfaces;
using IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Models;
using Services;
using System.Linq.Expressions;
using System.Security.Claims;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class DealsController : ControllerBase
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly IDashboardAnalyticsService _analyticsService;
	private readonly IHubContext<DashboardHub> _hubContext;

	public DealsController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IDashboardAnalyticsService analyticsService, IHubContext<DashboardHub> hubContext)
	{
		_unitOfWork = unitOfWork;
		_userManager = userManager;
		_analyticsService = analyticsService;
		_hubContext = hubContext;
	}

	[Authorize(Roles = "superadmin,agent")]
	[HttpGet("GetAllDeals")]
	public async Task<IActionResult> GetAllDeals([FromQuery] DealsFilterDto filter)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		var role = User.IsInRole("superadmin")? "superadmin" : "agent";

		if (string.IsNullOrEmpty(userId)) return Unauthorized();

		IEnumerable<Deal> deals;
		int totalCount = 0;
		IEnumerable<Object> dealsResponse = [];

		var includes = new string[] { "BuyerContact", "Property", "SellerContact", "Agent", "MeetingStatus", "BuyerConfirmationStatus", "SellerConfirmationStatus", "DealStatus" };

		switch (role)
		{
			case "superadmin":
				{
					Expression<Func<Deal, bool>> predicate = d => !filter.DealStatusId.HasValue || d.DealStatusId == (int)filter.DealStatusId.Value;

					var result = await _unitOfWork.Deals.GetPaginatedAsync(
						predicate,
						includes,
						q => q.OrderByDescending(d => d.DealDate),
						filter.PageNumber,
						filter.PageSize);

					deals = result.Items;
					totalCount = result.TotalCount;


					dealsResponse = deals.Select(d => new
					{
						DealId = d.DealId,
						Buyer = d.BuyerContact.Name,
						Seller = d.SellerContact.Name,
						Agent = d.Agent.Name,
						MeetingDate = d.MeetingDate.ToShortDateString(),
						MeetingLocation = d.MeetingLocation,
						MeetingStatus = d.MeetingStatus.Name,
						DealStatus = d.DealStatus.Name,
						FinalSaleAmount = d.FinalSaleAmount,
						DealDate = d.DealDate.ToShortDateString()
					});

					break;
				}
			case "agent":
				{
					Expression<Func<Deal, bool>> predicate = d => d.Agent.Id == Guid.Parse(userId) && (!filter.DealStatusId.HasValue || d.DealStatusId == (int)filter.DealStatusId.Value);

					var result = await _unitOfWork.Deals.GetPaginatedAsync(
						predicate,
						includes,
						q => q.OrderByDescending(d => d.DealDate),
						filter.PageNumber,
						filter.PageSize);

					deals = result.Items;
					totalCount = result.TotalCount;

					dealsResponse = deals.Select(d => new
					{
						DealId = d.DealId,
						Buyer = d.BuyerContact.Name,
						Seller = d.SellerContact.Name,
						MeetingDate = d.MeetingDate.ToShortDateString(),
						MeetingLocation = d.MeetingLocation,
						MeetingStatus = d.MeetingStatus.Name,
						DealStatus = d.DealStatus.Name,
						FinalSaleAmout = d.FinalSaleAmount,
						DealDate = d.DealDate.ToShortDateString()
					});
					break;
				}
		}
		var paginatedResult = new PaginatedResult<object>
		{
			Data = dealsResponse.ToList(),
			TotalCount = totalCount,
			PageNumber = filter.PageNumber,
			PageSize = filter.PageSize,
		};


		return Ok(paginatedResult);
	}

	[Authorize]
	[HttpPatch("UpdateDealStatusAfterCompletingMeeting/{id:int}")]
	public async Task<IActionResult> UpdateDealStatusAfterCompletingMeeting(int id)
	{
		var deal = _unitOfWork.Deals.FindOneItem(d => d.DealId == id, new string[] { "BuyerContact", "Property", "SellerContact", "Agent", "MeetingStatus", "BuyerConfirmationStatus", "SellerConfirmationStatus", "DealStatus" });

		if (deal == null)
		{
			return NotFound(new
			{
				status = "error",
				message = "We couldn't find the deal you're looking for. It may have been removed."
			});
		}

		deal.MeetingStatusId = 2; // Set to "Completed"
		deal.BuyerConfirmationStatusId = 2; // Set to "Confirmed"
		deal.SellerConfirmationStatusId = 2; // Set to "Confirmed"
		deal.DealStatusId = 2; // Set to "Completed"

		deal.Property.PropertyStatusId = 2; // Set to "Sold"
		deal.BuyerContact.ContactStatusId = 6; // set to "purchase_completed"
		deal.SellerContact.ContactStatusId = 6; // set to "purchase_completed"

		_unitOfWork.Deals.Update(deal);
		_unitOfWork.Save();

		// Build fresh analytics and push to all dashboard clients
		var analytics = await _analyticsService.BuildAnalyticsAsync(null);

		if (analytics != null)
			await _hubContext.Clients.Group("dashboard").SendAsync("ReceiveDashboardUpdate", analytics);

		return Ok(new
		{
			status = "success",
			message = "Deal updated successfully"
		});
	}

	[Authorize]
	[HttpPatch("CancelDeal/{id:int}")]
	public async Task<IActionResult> CancelDeal(int id)
	{
		var deal = _unitOfWork.Deals.FindOneItem(d => d.DealId == id, new string[] { "BuyerContact", "Property", "SellerContact", "Agent", "MeetingStatus", "BuyerConfirmationStatus", "SellerConfirmationStatus", "DealStatus" });

		if (deal == null)
		{
			return NotFound(new
			{
				status = "error",
				message = "We couldn't find the deal you're looking for. It may have been removed."
			});
		}

		deal.MeetingStatusId = 3; // Set to "Canceled"
		deal.BuyerConfirmationStatusId = 3; // Set to "Canceled"
		deal.SellerConfirmationStatusId = 3; // Set to "Canceled"
		deal.DealStatusId = 3; // Set to "Canceled"

		deal.BuyerContact.ContactStatusId = 4; // set to "not-interested"
		deal.SellerContact.ContactStatusId = 4; // set to "not-interested"

		_unitOfWork.Deals.Update(deal);
		_unitOfWork.Save();

		return Ok(new
		{
			status = "success",
			message = "Deal canceled successfully"
		});

	}

}

