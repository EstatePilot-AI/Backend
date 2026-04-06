using AI_ColdCall_Agent.Core.DTO;
using DTO;
using Interfaces;
using IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Models;
using RestSharp.Extensions;
using Services;

namespace Controllers;


[Route("api/[controller]")]
[ApiController]
public class PropertyController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
	private readonly IBackgroundTaskQueue _queue;
	private readonly EmailSender _emailSender;
	private readonly IWebHostEnvironment _env;

	public PropertyController(IUnitOfWork unitOfWork, IBackgroundTaskQueue queue, EmailSender emailSender, IWebHostEnvironment env)
    {
        _unitOfWork = unitOfWork;
		_queue = queue;
		_emailSender = emailSender;
		_env = env;
	}

    [HttpGet("GetPropertyById/{id}")]
    public async Task<IActionResult> GetPropertyById(int id)
    {
        var property = _unitOfWork.Properties.FindOneItem(
            p => p.PropertyId == id,
            new string[] {
            "PropertyType",
            "PropertyStatus",
            "FinishingType",
            "PropertiesLocation",
			"propertyImages"
			}
        );

        if (property == null)
        {
            return NotFound($"Property with ID {id} not found.");
        }

        var response = new PropertyListDto
        {
            PropertyId = property.PropertyId,
            PropertyType = property.PropertyType.Name,
            PropertyStatus = property.PropertyStatus.Name,
            FinishingType= property.FinishingType.Name,

            Price = property.Price,
            Area = property.Area,
            Rooms = property.Rooms,
            Bathrooms = property.Bathrooms,

            Country = property.PropertiesLocation?.Country,
            Governorate = property.PropertiesLocation?.Governorate,
            City = property.PropertiesLocation?.City,
            District = property.PropertiesLocation?.District,
            Street = property.PropertiesLocation?.Street,
            BuildingNumber = property.PropertiesLocation?.BuildingNumber ?? 0,
            FloorNumber = property.PropertiesLocation?.FloorNumber ?? 0,
            ApartmentNumber = property.PropertiesLocation?.ApartmentNumber ?? 0,

			ImageURLs = property.propertyImages?.Select(p => p.ImageURL).ToList()
		};

        return Ok(response);
    }

    [HttpGet("GetAllProperties")]
    public async Task<IActionResult> GetAllProperties()
    {
        var properties = await _unitOfWork.Properties.GetAllWithIncludesAsync(
            p => p.PropertyType,
            p => p.PropertyStatus,
            p => p.PropertiesLocation,
            p => p.propertyImages
        );

        if (properties == null || !properties.Any())
        {
            return Ok(new List<PropertyResponse>());
        }


        var propertyResponses = properties.Select(p => new PropertyResponse
        {
            PropertyId = p.PropertyId,
            Price = p.Price,
            Area = p.Area,

            PropertyType = p.PropertyType?.Name,
            Status = p.PropertyStatus?.Name,

            City = p.PropertiesLocation?.Country,
            District = p.PropertiesLocation?.City,

            ImageURLs = p.propertyImages?.Select(p => p.ImageURL).ToList()

        }).ToList();

        return Ok(propertyResponses);
    }

	[HttpPost("HandleCallOutcomeFromSeller")]
    public async Task<IActionResult> HandleCallOutcomeFromSeller([FromBody] AICallResultFromSeller resultDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var sellerContact = await _unitOfWork.Contacts.GetByIdAsync(int.Parse(resultDto.LeadID));

			if (sellerContact == null)
			{
				return NotFound(new
				{
					status = "error",
					error = new
					{
						message = $"Seller with ID {resultDto.LeadID} not found."
					}
				});
			}

            //To get the retryCount for the current call by counting the previous call logs for the same buyer
            var callLogForSameBuyer = (await _unitOfWork.CallLogs.FindAllAsync(cl => cl.ContactId == sellerContact.ContactId && cl.Contact.ContactTypeId == 2)).OrderByDescending(cl => cl.Timestamp).FirstOrDefault();

			int currentRetryCount = callLogForSameBuyer?.RetryCount ?? 0;

			var callLog = new CallLog
			{
				ContactId = sellerContact.ContactId,
				ContactName = resultDto.ContactName,
				SubjectTypeId = 2, //resales
				Transcript = resultDto.Summary,
				Timestamp = DateTime.UtcNow,
				Duration = (int)Math.Floor(resultDto.Duration),
				RetryCount = currentRetryCount,
				CallIDFromAI = resultDto.CallId
			};

			switch (resultDto.CallOutcome.ToLower())
			{
				case "interested":
					{
						callLog.CallSessionStateId = 1; //Answered
						callLog.CallOutcomeId = 1; // Interested

                        // Logic to add the property
                        string pTypeName = resultDto.propertyDTO.PropertyInfo.PropertyType?.Trim() ?? string.Empty;
                        string fTypeName = resultDto.propertyDTO.PropertyInfo.FinishingType?.Trim() ?? string.Empty;
                        string lTypeName = resultDto.propertyDTO.PropertyPayment.ListingType?.Trim() ?? string.Empty;
                        string sTypeName = resultDto.propertyDTO.PropertyPayment.PaymentMethod?.Trim() ?? string.Empty;


                        var pType = _unitOfWork.PropertyTypes.FindOneItem(t => t.Name.ToLower() == pTypeName.ToLower());
                        var fType = _unitOfWork.FinishingTypes.FindOneItem(f => f.Name.ToLower() == fTypeName.ToLower());
                        var lType = _unitOfWork.ListingTypes.FindOneItem(l => l.Name.ToLower() == lTypeName.ToLower());
                        var sType = _unitOfWork.PaymentMethods.FindOneItem(m => m.Name.ToLower() == sTypeName.ToLower());

                        var property = new Property
                        {
                            SellerContactId = sellerContact.ContactId,

                            PropertyTypeId = pType?.Id ?? 1,
                            FinishingTypeId = fType?.Id ?? 1,
                            ListingTypeId = lType?.ListingTypeId ?? 1,
                            PropertyStatusId = 1,
                            PaymentMethodId = sType?.Id ?? 1,

                            Negotiable= resultDto.propertyDTO.PropertyInfo.Negotiable ?? false,

							Price = resultDto.propertyDTO?.PropertyInfo?.Price ?? 0,
							Area = resultDto.propertyDTO?.PropertyInfo?.Area ?? 0,
							Rooms = resultDto.propertyDTO?.PropertyInfo?.Rooms ?? 0,
							Bathrooms = resultDto.propertyDTO?.PropertyInfo?.Bathrooms ?? 0,
							DownPayment = resultDto.propertyDTO?.PropertyPayment?.DownPayment ?? 0,
							Description = resultDto.propertyDTO?.PropertyInfo?.AdditionalInfo ?? string.Empty,

							PropertiesLocation = new PropertiesLocation
                            {
                                Country = resultDto.propertyDTO.PropertyLocation.Country ?? "مصر",
                                Governorate = resultDto.propertyDTO.PropertyLocation.Governorate ?? "",
                                City = resultDto.propertyDTO.PropertyLocation.City ?? "",
                                District = resultDto.propertyDTO.PropertyLocation.District ?? "",
                                Street = resultDto.propertyDTO.PropertyLocation.Street ?? "",
                                BuildingNumber = resultDto.propertyDTO.PropertyLocation.BuildingNumber ?? 0,
                                FloorNumber = resultDto.propertyDTO.PropertyLocation.FloorNumber ?? 0,
                                ApartmentNumber = resultDto.propertyDTO.PropertyLocation.ApartmentNumber ?? 0
                            }
                        };

						await _unitOfWork.Properties.AddAsync(property);
                        _unitOfWork.Save();

						//send email to notify the seller that the seller's property is added successfully
                        var message=HTMLMessages.ConfirmationEmailToSeller(property.PropertyId);
                        var subject = "Success: Your Property Has Been Added Successfully";
                        _emailSender.SendEmail(subject, sellerContact.Email, message);

						sellerContact.ContactStatusId = 2; //Qualified
						break;
					}
				case "notinterested":
					{
						callLog.CallSessionStateId = 1; //Answered
						callLog.CallOutcomeId = 2; // Not InterestedleadRequest.LeadRequestStatusId = 5; //Not interested
						sellerContact.ContactStatusId = 4; //Not Interested
						break;
					}
				case "noanswer":
				case "busy":
					{
						callLog.CallSessionStateId = 2; //Not Answered
						callLog.CallOutcomeId = 3; // Not Answered

						callLog.RetryCount = currentRetryCount + 1; //increment the retry count for the next call log entry

						if (currentRetryCount >= 3) //retry count for 3 times only
						{
							callLog.CallOutcomeId = 4; // Failed
							//_unitOfWork.Contacts.Delete(sellerContact);  //delete Invalid Number or failed to reach
						}
						else
						{
                            sellerContact.ContactStatusId = 5; //Retry pending
							_unitOfWork.Save();
							await _queue.QueueCallAsync(sellerContact.ContactId);
						}
						break;
					}
				case "failed":
					{
						callLog.CallSessionStateId = 2;//Not Answered
						callLog.CallOutcomeId = 4; // Failed

						//_unitOfWork.Contacts.Delete(sellerContact);  //delete contact if the call is failed
						break;
					}
			}
            await _unitOfWork.CallLogs.AddAsync(callLog);
			_unitOfWork.Save();

            //find the next available seller and enqueue it for calling
            //enable the worker to call when The AI is ready to the next call

            var nextSeller = (await _unitOfWork.Contacts.FindAllAsync(c => c.ContactStatusId == 1 || c.ContactStatusId == 5)).FirstOrDefault();

            if (nextSeller != null)
            {
                await Task.Delay(TimeSpan.FromSeconds(7));
                await _queue.QueueCallAsync(nextSeller.ContactId);
            }

			return Ok(new
			{
				status = "success",
				data = new
				{
					message = "Outcome processed successfully"
				}
			});
		}
        catch (Exception ex)
        {

            return BadRequest(new
            {
                error = "فشل في حفظ بيانات الـ AI",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [HttpPut("UpdateProperty/{id:int}")]
    public async Task<IActionResult> UpdateProperty(int id, UpdatePropertyToAddImages propertyDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existingProperty = _unitOfWork.Properties.FindOneItem(
            p => p.PropertyId == id,
            new string[] { "PropertiesLocation", "PropertyType", "PropertyStatus", "FinishingType", "propertyImages" }
        );

        if (existingProperty == null) return NotFound($"Property {id} not found.");


        existingProperty.PropertyTypeId = propertyDto.PropertyType;
        existingProperty.FinishingTypeId = propertyDto.FinishingType;
        existingProperty.Negotiable = propertyDto.Negotiable;

		existingProperty.Price = propertyDto.Price;
        existingProperty.Area = propertyDto.Area;
        existingProperty.Rooms = propertyDto.Rooms;
        existingProperty.Bathrooms = propertyDto.Bathrooms;

     
        if (existingProperty.PropertiesLocation != null)
        {
            existingProperty.PropertiesLocation.Country = propertyDto.Country;
            existingProperty.PropertiesLocation.Governorate = propertyDto.Governorate;
            existingProperty.PropertiesLocation.City = propertyDto.City;
            existingProperty.PropertiesLocation.District = propertyDto.District;
            existingProperty.PropertiesLocation.Street = propertyDto.Street;
            existingProperty.PropertiesLocation.BuildingNumber = propertyDto.BuildingNumber;
            existingProperty.PropertiesLocation.FloorNumber = propertyDto.FloorNumber;
            existingProperty.PropertiesLocation.ApartmentNumber = propertyDto.ApartmentNumber;
        }


        string folderName = Path.Combine(_env.WebRootPath, "Images");

        if(existingProperty.propertyImages != null)
        {
            foreach(var oldImage in existingProperty.propertyImages)
            {
                System.IO.File.Delete(folderName + oldImage.ImageURL);
            }
            existingProperty.propertyImages.Clear();
        }

        foreach(var file in propertyDto.ImageURLs)
        {
            var imageName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(folderName, imageName);

			using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var propertyImage = new PropertyImages
            {
                PropertyId = existingProperty.PropertyId,
                ImageURL = imageName
            };

            await _unitOfWork.PropertyImages.AddAsync(propertyImage);
		}

		_unitOfWork.Properties.Update(existingProperty);
        _unitOfWork.Save();

        return Ok(new
        {
            status = "success",
            data = new
            {
                message = $"Property {id} updated successfully."
			}
		}); 
    }

	[HttpGet("GlobalSearch")]
    public async Task<IActionResult> GlobalSearch([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest("Search term cannot be empty.");
        }

    
        var properties = await _unitOfWork.Properties.FindAllWithIncludeAsync(
            new string[] { "PropertyType", "FinishingType", "PropertiesLocation", "PropertyStatus" }
        );

       
        var filteredResults = properties.Where(p =>
            (p.PropertyType != null && p.PropertyType.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
            (p.FinishingType != null && p.FinishingType.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
            (p.PropertiesLocation != null && (
                p.PropertiesLocation.City.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                p.PropertiesLocation.Governorate.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                p.PropertiesLocation.District.Contains(term, StringComparison.OrdinalIgnoreCase)
            ))
        ).ToList();

        
        var response = filteredResults.Select(p => new PropertyResponse
        {
            PropertyId = p.PropertyId,
            Price = p.Price,
            Area = p.Area,
            PropertyType = p.PropertyType?.Name,
            Status = p.PropertyStatus?.Name,
            City = p.PropertiesLocation?.City,
            District = p.PropertiesLocation?.District
        }).ToList();

        return Ok(response);
    }
}




