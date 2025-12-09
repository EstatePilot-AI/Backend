using CsvHelper;
using DTO;
using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContactController : ControllerBase
{
	private readonly IUnitOfWork _unitOfWork;

	public ContactController(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	//Add and new contact to a buyer and add leadRequest by using buyerId and propertyId comes from the shop
	[HttpPost("AddBuyerContact{id:int}")]
	public async Task<IActionResult> AddBuyerContact(ContactDto contactDto, int id)
	{
		if (ModelState.IsValid)
		{
			if (id <= 0)
			{
				return BadRequest("Invalid buyer ID.");
			}
			var property = await _unitOfWork.Properties.GetByIdAsync(id);

			if (property == null) // check if property exists
			{
				return NotFound("Property not found.");
			}
			var contact = new Contact()
			{
				Name = contactDto.Name,
				Phone = contactDto.Phone,
				Email = contactDto.Email,
				ContactTypeId = 1,  //1 for buyer
				ContactStatusId = 1 //1 for pending_call
			};

			await _unitOfWork.Contacts.AddAsync(contact); //store contact for buyer
			_unitOfWork.Save();

			var leadRequest = new LeadRequest()
			{
				BuyerContactId = contact.ContactId,
				PropertyId = id,
				LeadRequestStatusId = 1 //1 for Pending Call
			};

			await _unitOfWork.LeadRequests.AddAsync(leadRequest); //store leadRequest
			_unitOfWork.Save();

			return Ok(new
			{
				Message = "Contact and Lead Request added successfully",
				BuyerContactId = contact.ContactId,
				LeadRequestId = leadRequest.RequestId
			});
		}
		return BadRequest(ModelState);
	}


	
}
