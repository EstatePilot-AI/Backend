using Interfaces;
using IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace Services;

public class AIOutboundCallWorker : BackgroundService
{
	private readonly IBackgroundTaskQueue _queue;
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly IHttpClientFactory _httpClientFactory;

	public AIOutboundCallWorker(IBackgroundTaskQueue queue, IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory)
	{
		_queue = queue;
		_scopeFactory = scopeFactory;
		_httpClientFactory = httpClientFactory;
	}
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// Continuously process the database queue until the service is stopped
		//await ProcessDatabaseQueue(stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			var leadRequestId = await _queue.DequeueAsync(stoppingToken);

			await ExecuteCallLogic(leadRequestId);

			await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
		}
	}

	private async Task ProcessDatabaseQueue(CancellationToken stoppingToken)
	{
		using var scope =_scopeFactory.CreateScope();
		var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
		var pendingCalls = await unitOfWork.LeadRequests.FindAllAsync(lr => lr.LeadRequestStatusId == 1 || lr.LeadRequestStatusId==6); //Get all pending calls and retry pending calls in leadRequests table

		foreach(var leadRequest in pendingCalls)
		{
			if(stoppingToken.IsCancellationRequested)
				break;

			await _queue.QueueCallAsync(leadRequest.RequestId);
		}
	}

	private async Task ExecuteCallLogic(int leadRequestId)
	{
		using var scope = _scopeFactory.CreateScope();
		var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

		var request = unitOfWork.LeadRequests.FindOneItem(lr => lr.RequestId == leadRequestId, new string[] { "Contact", "Property", "Property.PropertyType", "Property.FinishingType", "Property.PropertiesLocation", "LeadRequestStatus" });

		if (request == null) return;

		bool shouldcall = false;

		if (request.LeadRequestStatusId == 1) //initial pending call, never called before
		{
			shouldcall = true;
		}
		else if (request.LeadRequestStatusId == 6) //retry pending call
		{
			var callLogs = await unitOfWork.CallLogs.FindAllAsync(cl => cl.ContactId == request.BuyerContactId);
			var lastCall = callLogs.OrderByDescending(cl => cl.Timestamp).FirstOrDefault();

			if (lastCall != null && (DateTime.UtcNow - lastCall.Timestamp).TotalHours >= 2)
			{
				shouldcall = true;
			}
		}

		if (shouldcall)
		{
			request.LeadRequestStatusId = 2; // set status to initiated
			unitOfWork.Save();
			await TriggerExternalCall(request);
		}
	}
	private async Task TriggerExternalCall(LeadRequest leadRequest)
	{
		var client = _httpClientFactory.CreateClient();

		var requestbody = new
		{
			callType = "sales", // Lowercase to match your target JSON
			leadInfo = new
			{
				id = leadRequest.RequestId,
				name = leadRequest.Contact.Name,
				phone = leadRequest.Contact.Phone
			},
			propInfo = new
			{
				type = leadRequest.Property.PropertyType.Name,
				finishing = leadRequest.Property.FinishingType.Name,
				price = leadRequest.Property.Price.ToString(),
				area = leadRequest.Property.Area.ToString(),
				rooms = leadRequest.Property.Rooms.ToString(),
				bathrooms = leadRequest.Property.Bathrooms.ToString(),
				location = new
				{
					country = leadRequest.Property.PropertiesLocation.Country,
					governorate = leadRequest.Property.PropertiesLocation.Governorate,
					city = leadRequest.Property.PropertiesLocation.City,
					street = leadRequest.Property.PropertiesLocation.Street,
					building = leadRequest.Property.PropertiesLocation.District,
					buildingNumber = leadRequest.Property.PropertiesLocation.BuildingNumber,
					floor = leadRequest.Property.PropertiesLocation.FloorNumber,
					apartmentNumber = leadRequest.Property.PropertiesLocation.ApartmentNumber
				},
				additionalInfo = leadRequest.Property.Description // Explicitly set to null to match your sample
			}
		};

		await client.PostAsJsonAsync("https://uncrumbled-ena-unmouthable.ngrok-free.dev/api/v1/call", requestbody);
	}
}
