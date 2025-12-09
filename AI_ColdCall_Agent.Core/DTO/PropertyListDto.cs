namespace DTO;

public class PropertyListDto
{
    public int PropertyId { get; set; }
    public string PropertyType { get; set; }
    public string PropertyStatus { get; set; }
    public string FinishingType { get; set; }

    public decimal Price { get; set; }
    public decimal Area { get; set; }
    public int Rooms { get; set; }
    public int Bathrooms { get; set; }
    public string Country { get; set; }
    public string Governorate { get; set; }
    public string City { get; set; }
    public string District { get; set; }
    public string Street { get; set; }
    public int BuildingNumber { get; set; }
    public int FloorNumber { get; set; }
    public int ApartmentNumber { get; set; }
}
