using Interfaces;
using IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Reflection.Metadata;
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
		//Start the database sync in a separate Task so it doesn't block the Dequeue loop

		// Sync existing pending leads from DB every 5 minutes as a fallback
		_ = Task.Run(async () => {
			while (!stoppingToken.IsCancellationRequested)
			{
				await ProcessDatabaseQueue(stoppingToken);
				await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
			}
		}, stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			var leadRequestId = await _queue.DequeueAsync(stoppingToken);

			await ExecuteCallLogic(leadRequestId, stoppingToken);

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

	private async Task ExecuteCallLogic(int leadRequestId, CancellationToken ct)
	{
		using var scope = _scopeFactory.CreateScope();
		var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

		//Handle Race Condition(Wait if DB hasn't committed yet)
		LeadRequest? request = null;
		for(int i = 0; i < 10; i++)
		{
			request = unitOfWork.LeadRequests.FindOneItem(lr => lr.RequestId == leadRequestId, new string[] { "Contact", "Property", "Property.PropertyType", "Property.FinishingType", "Property.PropertiesLocation", "LeadRequestStatus" });

			if (request != null) break;

			await Task.Delay(1000, ct);
		}


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
				type = leadRequest.Property?.PropertyType?.Name ?? "",
				finishing = leadRequest.Property?.FinishingType?.Name ?? "",
				price = leadRequest.Property?.Price.ToString() ?? "0",
				area = leadRequest.Property?.Area.ToString() ?? "0",
				rooms = leadRequest.Property?.Rooms.ToString() ?? "0",
				bathrooms = leadRequest.Property?.Bathrooms.ToString() ?? "0",
				negotiable= leadRequest.Property?.Negotiable ?? false,
				location = new
				{
					country = leadRequest.Property?.PropertiesLocation?.Country ?? "مصر",
					governorate = leadRequest.Property?.PropertiesLocation?.Governorate ?? "",
					city = leadRequest.Property?.PropertiesLocation?.City ?? "",
					street = leadRequest.Property?.PropertiesLocation?.Street ?? "",
					building = leadRequest.Property?.PropertiesLocation?.District ?? "",
					buildingNumber = leadRequest.Property?.PropertiesLocation?.BuildingNumber ?? 0,
					floor = leadRequest.Property?.PropertiesLocation?.FloorNumber ?? 0,
					apartmentNumber = leadRequest.Property?.PropertiesLocation?.ApartmentNumber ?? 0
				},
				additionalInfo = leadRequest.Property?.Description ?? "" // Explicitly set to null to match your sample
			}
		};

		var response=await client.PostAsJsonAsync("https://call-handler.vercel.app/api/v1/call", requestbody);

		response.EnsureSuccessStatusCode();
	}
}
