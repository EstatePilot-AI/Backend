using DTO;
using Identity;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Security.Claims;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class DealsController : ControllerBase
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly UserManager<ApplicationUser> _userManager;

	public DealsController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
	{
		_unitOfWork = unitOfWork;
		_userManager = userManager;
	}

	[Authorize(Roles = "superadmin,agent")]
	[HttpGet("GetAllDeals")]
	public async Task<IActionResult> GetAllDeals([FromQuery] DealStatus? dealStatusId)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		var role = User.IsInRole("superadmin")? "superadmin" : "agent";

		if (string.IsNullOrEmpty(userId)) return Unauthorized();

		IEnumerable<Deal> deals;
		IEnumerable<Object> dealsResponse = [];
		switch (role)
		{
			case "superadmin":
				{
					deals = await _unitOfWork.Deals.FindAllWithIncludeAsync(new string[] { "BuyerContact", "Property", "SellerContact", "Agent", "MeetingStatus", "BuyerConfirmationStatus", "SellerConfirmationStatus", "DealStatus" });
					
					if (dealStatusId.HasValue)
					{
						deals = deals.Where(d => d.DealStatusId == (int)dealStatusId.Value);
					}


					dealsResponse = deals?.OrderByDescending(d => d.DealDate).Select(d => new
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
					deals = await _unitOfWork.Deals.FindAllAsync(d => d.Agent.Id == Guid.Parse(userId), new string[] { "BuyerContact", "Property", "SellerContact", "Agent", "MeetingStatus", "BuyerConfirmationStatus", "SellerConfirmationStatus", "DealStatus" });

					if (dealStatusId.HasValue)
					{
						deals = deals.Where(d => d.DealStatusId == (int)dealStatusId.Value);
					}


					dealsResponse = deals?.OrderByDescending(d => d.DealDate).Select(d => new
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


		return Ok(dealsResponse);
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

public enum DealStatus
{
	InProgress = 1,
	Completed = 2,
	Canceled = 3
}
