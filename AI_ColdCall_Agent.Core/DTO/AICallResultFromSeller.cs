using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DTO;

public class AICallResultFromSeller
{
	public string LeadID { get; set; }
	public string ContactName { get; set; }
	public string CallId { get; set; }
	public string Summary { get; set; }
	public double Duration { get; set; }
	public string CallOutcome { get; set; } //Interested, notInterested, noAnswer or busy

	public PropertyDTO? propertyDTO { get; set; }
}

public class PropertyDTO
{
	public PropertyInfoDto? PropertyInfo { get; set; }
	public PropertyPaymentDto? PropertyPayment { get; set; }
	public PropertyLocationDto? PropertyLocation { get; set; }
}

public class PropertyInfoDto
{
	public string? PropertyType { get; set; } // Options: Apartment, Villa, Studio, Chalet, Duplex, Townhouse
	public decimal? Price { get; set; }
	public decimal? Area { get; set; }
	public int? Rooms { get; set; }
	public int? Bathrooms { get; set; }

	public string? FinishingType { get; set; }  // Options: Without Finishing, Semi-Finished, Fully Finished, Super Lux

	[DefaultValue(false)]
	public bool? Negotiable { get; set; } = false;
	public string? AdditionalInfo { get; set; }
}

public class PropertyPaymentDto
{
	public decimal? DownPayment { get; set; }

	public string? PaymentMethod { get; set; }  // Options: Cash, Installments

	public string? ListingType { get; set; }   // Options: For Sale, For Rent
}

public class PropertyLocationDto
{
	public string? Country { get; set; }
	public string? Governorate { get; set; }
	public string? City { get; set; }
	public string? District { get; set; }
	public string? Street { get; set; }
	public int? BuildingNumber { get; set; }
	public int? FloorNumber { get; set; }
	public int? ApartmentNumber { get; set; }
}