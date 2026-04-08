using DTO;
using Identity;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;

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

	[Authorize(Roles = "superadmin")]
	[HttpGet("GetAllDeals")]
	public async Task<IActionResult> GetAllDeals()
	{
		var deals = await _unitOfWork.Deals.FindAllWithIncludeAsync(new string[] { "BuyerContact", "Property", "SellerContact", "Agent", "MeetingStatus", "BuyerConfirmationStatus", "SellerConfirmationStatus", "DealStatus" });


		var dealsResponse = deals?.OrderBy(d => d.DealDate).Select(d => new DealDto
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

		return Ok(dealsResponse);
	}

	[Authorize]
	[HttpGet("GetAllDealsPerAgent")]
	public async Task<IActionResult> GetAllDealsPerAgent()
	{
		var agent = await _userManager.GetUserAsync(User);

		if (agent == null)
		{
			return NotFound(new
			{
				status = "error",
				message = "Agent is not found"
			});
		}

		var deals = await _unitOfWork.Deals.FindAllAsync(d => d.Agent.Id == agent.Id, new string[] { "BuyerContact", "Property", "SellerContact", "Agent", "MeetingStatus", "BuyerConfirmationStatus", "SellerConfirmationStatus", "DealStatus" });


		var dealsResponse = deals?.OrderBy(d => d.DealDate).Select(d => new
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

		return Ok(dealsResponse);
	}

	[Authorize(Roles ="superadmin")]
	[HttpGet("FilterDealsByStatus/{id:int}")]
	public async Task<IActionResult> FilterDealsByStatus(int id)
	{
		var deals = await _unitOfWork.Deals.FindAllAsync(d => d.DealStatusId == id, new string[] { "BuyerContact", "Property", "SellerContact", "Agent", "MeetingStatus", "BuyerConfirmationStatus", "SellerConfirmationStatus", "DealStatus" });

		var dealsResponse = deals?.OrderBy(d => d.DealDate).Select(d => new DealDto
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

		return Ok(dealsResponse);
	}

	[Authorize]
	[HttpGet("FilterDealsByStatusPerAgent/{id:int}")]
	public async Task<IActionResult> FilterDealsByStatusPerAgent(int id)
	{
		var agent = await _userManager.GetUserAsync(User);
		if (agent == null)
		{
			return NotFound(new
			{
				status = "error",
				message = "Agent is not found"
			});
		}
		var deals = await _unitOfWork.Deals.FindAllAsync(d => d.Agent.Id == agent.Id && d.DealStatusId == id, new string[] { "BuyerContact", "Property", "SellerContact", "Agent", "MeetingStatus", "BuyerConfirmationStatus", "SellerConfirmationStatus", "DealStatus" });

		var dealsResponse = deals?.OrderBy(d => d.DealDate).Select(d => new
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
				message = "Deal is not found"
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
				message = "Deal is not found"
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
