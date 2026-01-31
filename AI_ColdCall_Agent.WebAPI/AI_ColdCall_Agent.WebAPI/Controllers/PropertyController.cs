using AI_ColdCall_Agent.Core.DTO;
using DTO;
using Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using RestSharp.Extensions;


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
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProperty(int id, [FromBody] PropertyListDto propertyDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existingProperty = _unitOfWork.Properties.FindOneItem(
            p => p.PropertyId == id,
            new string[] { "PropertiesLocation", "PropertyType", "PropertyStatus", "FinishingType" }
        );

        if (existingProperty == null) return NotFound($"Property {id} not found.");

       
        var typeEntity = _unitOfWork.PropertyTypes.FindOneItem(t => t.Name == propertyDto.PropertyType);
        if (typeEntity != null) existingProperty.PropertyTypeId = typeEntity.Id;

        var statusEntity = _unitOfWork.PropertyStatuses.FindOneItem(s => s.Name == propertyDto.PropertyStatus);
        if (statusEntity != null) existingProperty.PropertyStatusId = statusEntity.Id;

        var finishEntity = _unitOfWork.FinishingTypes.FindOneItem(f => f.Name == propertyDto.FinishingType);
        if (finishEntity != null) existingProperty.FinishingTypeId = finishEntity.Id;

       
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

    
        _unitOfWork.Properties.Update(existingProperty);
        _unitOfWork.Save();

        return NoContent(); 
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




