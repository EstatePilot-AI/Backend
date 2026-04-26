using CsvHelper;
using DTO;
using Interfaces;
using IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContactController : ControllerBase
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IBackgroundTaskQueue _queue;

	public ContactController(IUnitOfWork unitOfWork, IBackgroundTaskQueue queue)
	{
		_unitOfWork = unitOfWork;
		_queue = queue;
	}

	//Add and new contact to a buyer and add leadRequest by using buyerId and propertyId comes from the shop
	[HttpPost("AddBuyerContact{id:int}")]
	public async Task<IActionResult> AddBuyerContact(ContactDto contactDto, int id)
	{
		if (ModelState.IsValid)
		{
			if (id <= 0)
			{
				return BadRequest(new
				{
					status = "error",
					error = new
					{
						message = "The buyer ID provided is not valid. Please check and try again."
					}
				});
			}
			var property = await _unitOfWork.Properties.GetByIdAsync(id);

			if (property == null) // check if property exists
			{
				return NotFound(new
				{
					status = "error",
					error = new
					{
						message = "We couldn't find the property you're interested in. It may no longer be available."
					}
				});
			}

			var contact = new Contact();

			var existingContact = _unitOfWork.Contacts.FindOneItem(c => c.Phone == contactDto.Phone); // check if the contact already exists for buyer

			if (existingContact != null)
			{
				// check if the existing contact already has a lead request for the same property
				var existingLeadRequest = (await _unitOfWork.LeadRequests.FindAllAsync(lr => lr.BuyerContactId == existingContact.ContactId && lr.PropertyId == property.PropertyId)).FirstOrDefault();

				if (existingLeadRequest != null)
				{
					return BadRequest(new
					{
						status = "error",
						Message = "You have already submitted an interest request for this property.",
						LeadRequestId= existingLeadRequest.RequestId
					});
				}
				
				existingContact.Name= contactDto.Name; // update name
				existingContact.Email= contactDto.Email; // update email
				existingContact.ContactStatusId = 1; // if the contact is existing into system return the status to pending_call
				existingContact.ContactTypeId = 1; //buyer
			}
			else
			{
				contact = new Contact()
				{
					Name = contactDto.Name,
					Phone = contactDto.Phone,
					Email = contactDto.Email,
					ContactTypeId = 1,  //1 for buyer
					ContactStatusId = 1 //1 for pending_call
				};

				await _unitOfWork.Contacts.AddAsync(contact); //store contact for buyer

				existingContact = contact; // for leadRequest creation
			}
				
			_unitOfWork.Save();

			var leadRequest = new LeadRequest()
			{
				BuyerContactId = existingContact.ContactId,
				BuyerName= existingContact.Name,
				PropertyId = id,
				LeadRequestStatusId = 1 //1 for Pending Call
			};

			await _unitOfWork.LeadRequests.AddAsync(leadRequest); //store leadRequest
			_unitOfWork.Save();

			//Add requestId to the queue
			await _queue.QueueCallAsync(leadRequest.RequestId);

			return Ok(new
			{
				Status = "success",
				Message = "Contact and Lead Request added successfully",
				BuyerContactId = existingContact.ContactId,
				LeadRequestId = leadRequest.RequestId
			});
		}
		return BadRequest(ModelState);
	}

	[Authorize(Roles = "superadmin")]
	[HttpPost("UploadCSVForSellers")]
	public async Task<IActionResult> UploadCSVForSellers(IFormFile file)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		if (file == null || file.Length == 0)
		{
			return BadRequest("Please select a CSV file to upload.");
		}

		List<ContactDto> allRecords;
		try
		{
			using (var stream = new StreamReader(file.OpenReadStream()))
			using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
			{
				allRecords = csv.GetRecords<ContactDto>().ToList();
			}
		}
		catch (Exception)
		{
			return BadRequest("The uploaded file appears to be invalid or corrupted.");
		}

		// 1. Clean data: Remove whitespace and filter out null phones/emails
		// This prevents " 123" being treated differently than "123"
		foreach (var record in allRecords)
		{
			record.Phone = record.Phone?.Trim();
			record.Email = record.Email?.Trim();
		}

		// 2. Pre-fetch existing sellers from DB (checking BOTH Phone and Email)
		var incomingPhones = allRecords.Where(p => !string.IsNullOrEmpty(p.Phone)).Select(p => p.Phone).ToList();
		var incomingEmails = allRecords.Where(e => !string.IsNullOrEmpty(e.Email)).Select(e => e.Email).ToList();

		var existingSellers = await _unitOfWork.Contacts.FindAllAsync(s =>
			incomingPhones.Contains(s.Phone) || incomingEmails.Contains(s.Email));

		// Create dictionaries for fast lookup
		var existingPhonesDict = existingSellers.Where(s => s.Phone != null).ToDictionary(s => s.Phone, s => s);
		var existingEmailsDict = existingSellers.Where(s => s.Email != null).ToDictionary(s => s.Email, s => s);

		int successCount = 0;
		List<string> errors = new List<string>();

		foreach (var csvRecord in allRecords)
		{
			try
			{
				// 1. Validation
				var context = new ValidationContext(csvRecord);
				var validationResults = new List<ValidationResult>();
				if (!Validator.TryValidateObject(csvRecord, context, validationResults, true))
				{
					errors.Add($"Row skipped (Validation): {csvRecord.Name} - {validationResults[0].ErrorMessage}");
					continue;
				}

				Contact sellerByPhone = null;
				Contact sellerByEmail = null;

				bool existsByPhone = !string.IsNullOrEmpty(csvRecord.Phone) &&
									 existingPhonesDict.TryGetValue(csvRecord.Phone, out sellerByPhone);

				bool existsByEmail = !string.IsNullOrEmpty(csvRecord.Email) &&
									 existingEmailsDict.TryGetValue(csvRecord.Email, out sellerByEmail);

				// --- OVERWRITE LOGIC ---
				if (existsByPhone)
				{
					// The phone exists, so this is our primary record to update.
					var existing = sellerByPhone;

					// If the CSV provides a new email that is currently owned by SOMEONE ELSE...
					if (existsByEmail && sellerByEmail.ContactId != sellerByPhone.ContactId)
					{
						// the email from the OTHER person before giving it to this person.
						sellerByEmail.Email = "X@yahoo.com";
					}

					// Overwrite with new data from CSV
					existing.Name = csvRecord.Name;
					existing.Email = csvRecord.Email; // Take the new email
					existing.ContactStatusId = 1; // Set to pending_call
					existing.ContactTypeId = 2; //seller

					// Update the email dictionary so subsequent rows know this email is now taken by 'existing'
					if (!string.IsNullOrEmpty(existing.Email))
					{
						existingEmailsDict[existing.Email] = existing;
					}

					continue;
				}
				else if (existsByEmail)
				{
					// Phone is new, but Email exists. 
					// we update the person who has this email with the new phone.
					var existing = sellerByEmail;

					existing.Name = csvRecord.Name;
					existing.Phone = csvRecord.Phone; // Take the new phone
					existing.ContactStatusId = 1; // Set to pending_call
					existing.ContactTypeId = 2; //seller


					// Update phone dictionary for subsequent rows
					if (!string.IsNullOrEmpty(existing.Phone))
					{
						existingPhonesDict[existing.Phone] = existing;
					}

					continue;
				}

				// 4. Create New Seller (only if neither phone nor email exists)
				var newSeller = new Contact()
				{
					Name = csvRecord.Name,
					Phone = csvRecord.Phone,
					Email = csvRecord.Email,
					ContactTypeId = 2, //seller
					ContactStatusId = 1 //pending_call
				};

				await _unitOfWork.Contacts.AddAsync(newSeller);

				if (!string.IsNullOrEmpty(newSeller.Phone))
					existingPhonesDict.TryAdd(newSeller.Phone, newSeller);

				if (!string.IsNullOrEmpty(newSeller.Email))
					existingEmailsDict.TryAdd(newSeller.Email, newSeller);

				successCount++;
			}
			catch (Exception ex)
			{
				errors.Add($"Failed to process {csvRecord.Phone}: {ex.Message}");
			}
		}

		try
		{
			_unitOfWork.Save();
		}
		catch (DbUpdateException ex)
		{
			return BadRequest("Database error: A unique constraint was violated that wasn't caught by the pre-check.");
		}

		return Ok(new
		{
			TotalRead = allRecords.Count,
			Saved = successCount,
			Errors = errors
		});
	}

}
