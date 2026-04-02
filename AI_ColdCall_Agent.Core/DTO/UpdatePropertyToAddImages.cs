using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace DTO;

public class UpdatePropertyToAddImages
{
	public int PropertyType { get; set; }
	public int FinishingType { get; set; }
	public bool Negotiable { get; set; }
	public decimal Price { get; set; }
	public decimal Area { get; set; }
	public int Rooms { get; set; }
	public int Bathrooms { get; set; }
	public string? Country { get; set; }
	public string Governorate { get; set; }
	public string City { get; set; }
	public string District { get; set; }
	public string Street { get; set; }
	public int BuildingNumber { get; set; }
	public int FloorNumber { get; set; }
	public int ApartmentNumber { get; set; }

	public List<IFormFile> ImageURLs { get; set; }
}
