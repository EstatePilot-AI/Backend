using AI_ColdCall_Agent.Core.DTO;
using DTO;
using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace Controllers;


[Route("api/[controller]")]
[ApiController]
public class PropertyController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;


    public PropertyController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
            "PropertiesLocation"
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
            ApartmentNumber = property.PropertiesLocation?.ApartmentNumber ?? 0
        };

        return Ok(response);
    }






    [HttpGet("GetAllProperties")]
    public async Task<IActionResult> GetAllProperties()
    {
        var properties = await _unitOfWork.Properties.FindAllWithIncludeAsync(
           new string[] {
            "PropertyType",
            "PropertyStatus",
            "PropertiesLocation"
            }
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
            District = p.PropertiesLocation?.City

        }).ToList();

        return Ok(propertyResponses);
    }




    [HttpPost]
    public async Task<IActionResult> CreateProperty([FromBody] PropertyDto propertyDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }


     


        var property = new Property
        {
            SellerContactId = propertyDto.SellerContactId,
            PropertyTypeId = propertyDto.PropertyTypeId,
            PropertyStatusId = propertyDto.PropertyStatusId,
            Price = propertyDto.Price,
            Area = propertyDto.Area,
            Rooms = propertyDto.Rooms,
            Bathrooms = propertyDto.Bathrooms,
            DownPayment = propertyDto.DownPayment,
            PaymentMethodId = propertyDto.PaymentMethodId,
            FinishingTypeId = propertyDto.FinishingTypeId,
            ListingTypeId = propertyDto.ListingTypeId,
            Description = propertyDto.Description,
            PropertiesLocation =new PropertiesLocation
            {
                Country = propertyDto.Country,
                Governorate = propertyDto.Governorate,
                City = propertyDto.City,
                District = propertyDto.District,
                Street = propertyDto.Street,
                BuildingNumber = propertyDto.BuildingNumber,
                FloorNumber = propertyDto.FloorNumber,
                ApartmentNumber = propertyDto.ApartmentNumber

            }
        };


        await _unitOfWork.Properties.AddAsync(property);
        _unitOfWork.Save();


        return CreatedAtAction(nameof(CreateProperty), new { id = property.PropertyId }, property);
    }
}



