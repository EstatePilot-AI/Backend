using DTO;
using Interfaces;
using Microsoft.AspNetCore.Http;
using Models;
using Microsoft.AspNetCore.Mvc;

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



