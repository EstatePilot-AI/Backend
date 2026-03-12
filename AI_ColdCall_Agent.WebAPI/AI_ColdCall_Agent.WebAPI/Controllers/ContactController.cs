using CsvHelper;
using DTO;
using Interfaces;
using IServices;
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
						message = "Invalid buyer Id"
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
						message = "Property not found"
					}
				});
			}

			var contact = new Contact();

			var existingContact = _unitOfWork.Contacts.FindOneItem(c => c.Phone == contactDto.Phone && c.ContactTypeId == 1); // check if the contact already exists for buyer

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


	[HttpPost("UploadCSVForSellers")]
	public async Task<IActionResult> UploadCSVForSellers(IFormFile file)
	{
		if (ModelState.IsValid)
		{
			if (file == null || file.Length == 0)
			{
				return BadRequest("No file uploaded.");
			}

			//Read All data from CSV file
			List<ContactDto> allRecords;

			try
			{
				using (var stream = new StreamReader(file.OpenReadStream()))
				{
					using (var csv = new CsvReader(stream, CultureInfo.InvariantCulture))
					{
						//Get all records from csv file and put them into list
						allRecords = csv.GetRecords<ContactDto>().ToList();
					}
				}
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}


			//Extract all distinct phone numbers from the CSV records
			var incomingPhones = allRecords.Where(p => p.Phone != null).Select(p => p.Phone).Distinct().ToList();

			//Get All sellers from DB whose phone numbers are in the incomingPhones list
			var existingSellers = await _unitOfWork.Contacts.FindAllAsync(s => s.ContactTypeId == 2 && incomingPhones.Contains(s.Phone));

			var existingSellersDictionary = existingSellers.ToDictionary(s => s.Phone, s => s);


			int successCount = 0;
			List<string> errors = new List<string>();


			foreach (var csvRecord in allRecords)
			{

				try
				{
					//var seller = _unitOfWork.Contacts.FindOneItem(s => s.Phone == csvRecord.Phone);

					//if (seller != null) //check if the seller is existing in the system
					//{
					//	seller.ContactStatusId = 1; // if the seller is existing into system return the status to pending_call
					//	continue;
					//}

					// Validate Format for name and phonenumber
					var context = new ValidationContext(csvRecord);
					var validationResults = new List<ValidationResult>();
					if (!Validator.TryValidateObject(csvRecord, context, validationResults, true))
					{
						errors.Add($"Row skipped (Validation): {csvRecord.Name} - {validationResults[0].ErrorMessage}");
						continue;
					}

					//Check Duplicates in phone numbers
					if (existingSellersDictionary.TryGetValue(csvRecord.Phone, out var existingSeller))
					{
						existingSeller.ContactStatusId = 1; // if the seller is existing into system return the status to pending_call

						_unitOfWork.Save();

						errors.Add($"Row skipped (Duplicate): {csvRecord.Phone} exists.");
						continue;
					}

					var newSeller = new Contact()
					{
						Name = csvRecord.Name,
						Phone = csvRecord.Phone,
						Email = csvRecord.Email,
						ContactTypeId = 2, //2 for Seller
						ContactStatusId = 1 //1 for pending_call
					};

					await _unitOfWork.Contacts.AddAsync(newSeller);

					//add new seller to existingSellersDictionary to avoid duplicates in the same CSV upload
					existingSellersDictionary.Add(newSeller.Phone, newSeller);

					successCount++;  //increment the count of actually new sellers
				}
				catch (Exception ex)
				{
					errors.Add($"Failed to save {csvRecord.Email}: {ex.Message}");
				}
			}

			_unitOfWork.Save(); //save all new sellers int DB

			//Trigger the first call only
			var firstSeller=(await _unitOfWork.Contacts.FindAllAsync(c=>c.ContactTypeId==2 && c.ContactStatusId==1)).OrderBy(c=>c.ContactId).FirstOrDefault();

			if(firstSeller != null)
			{
				await _queue.QueueCallAsync(firstSeller.ContactId);
			}

			return Ok(new
			{
				TotalRead = allRecords.Count(),
				Save = successCount,
				Errors = errors
			});
		}

		return BadRequest(ModelState);
	}

	[HttpPut("UpdateContactStatus{id:int}")]
	public async Task<IActionResult> UpdateContactStatus(int id, string status)
	{
		if (ModelState.IsValid)
		{
			if (id <= 0)
			{
				return BadRequest("Invalid contact ID.");
			}

			var contact = _unitOfWork.Contacts.FindOneItem(c => c.ContactId == id, new string[] { "ContactStatus", "ContactStatus" });

			if (contact == null)
			{
				return NotFound("The contact isn't found in the system");
			}

			
		}
		return BadRequest(ModelState);
	}

}
