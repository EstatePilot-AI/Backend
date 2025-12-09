using System;
using System.Collections.Generic;
using System.Text;

namespace DTO;

public class PropertyDto
{
      public int SellerContactId { get; set; }
    public int PropertyTypeId { get; set; }
    public int PropertyStatusId { get; set; }
    public decimal Price { get; set; }
    public decimal Area { get; set; }
    public int Rooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal DownPayment { get; set; }
    public int PaymentMethodId { get; set; }
    public int FinishingTypeId { get; set; }
    public int ListingTypeId { get; set; }
    public string Description { get; set; }


    // Properties from PropertiesLocation
    public string Country { get; set; }
    public string Governorate { get; set; }
    public string City { get; set; }
    public string District { get; set; }
    public string Street { get; set; }
    public int BuildingNumber { get; set; }
    public int FloorNumber { get; set; }
    public int ApartmentNumber { get; set; }


}
public class PropertyResponceDto
{ public int PropertyId { get; set; }
    public int PropertyTypeId { get; set; }
    public int PropertyStatusId { get; set; }
    public decimal Price { get; set; }
    public decimal Area { get; set; }
    public string Governorate { get; set; }

}
