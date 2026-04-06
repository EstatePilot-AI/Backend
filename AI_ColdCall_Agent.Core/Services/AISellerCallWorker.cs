using Interfaces;
using IServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace Services;

public class AISellerCallWorker : BackgroundService
{
	private readonly IBackgroundTaskQueue _queue;
	private readonly IServiceScopeFactory _scopeFactory;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger<AISellerCallWorker> _logger;

	public AISellerCallWorker(IBackgroundTaskQueue queue, IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, ILogger<AISellerCallWorker> logger)
	{
		_queue = queue;
		_scopeFactory = scopeFactory;
		_httpClientFactory = httpClientFactory;
		_logger = logger;
	}
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			// Wait for the signal from CSV upload or previous call outcome
			var sellerId = await _queue.DequeueAsync(stoppingToken);

			await ExecuteSellerCallLogic(sellerId, stoppingToken);

		}
	}

	private async Task ExecuteSellerCallLogic(int sellerId, CancellationToken ct)
	{
		using var scope = _scopeFactory.CreateScope();
		var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

		//Retry lookup. The DB might be slow to commit the CSV rows.
		Contact? seller = null;
		for(int i = 0; i < 10; i++)
		{
			//Get seller contact
			seller = unitOfWork.Contacts.FindOneItem(c => c.ContactId == sellerId && c.ContactTypeId == 2); //2 refers to seller contact type
			if (seller != null) break;
			await Task.Delay(1000, ct); // Wait 1s and try again
		}
		
		if (seller == null) return;

		bool shouldCall = false;

		if (seller.ContactStatusId==1) //pending
		{
			shouldCall = true;
		}
		else if(seller.ContactStatusId==5) //retry pending
		{
			var callLogs = await unitOfWork.CallLogs.FindAllAsync(cl => cl.ContactId == seller.ContactId);
			var lastCall = callLogs.OrderByDescending(cl => cl.Timestamp).FirstOrDefault();

			if (lastCall != null && (DateTime.UtcNow - lastCall.Timestamp).TotalHours >= 2)
			{
				shouldCall = true;
			}
		}

		if (shouldCall)
		{
			try
			{
				_logger.LogInformation("Initiating AI call for Seller {Id}...", sellerId);

				seller.ContactStatusId = 7; //initiated - call in progress
				unitOfWork.Save();

				await TriggerExternalCall(seller);

				_logger.LogInformation("Successfully triggered API for Seller {Id}.", sellerId);

				// only if a call was actually made.
				await Task.Delay(TimeSpan.FromSeconds(2), ct);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Failed to trigger external call for Seller {Id}. Reverting status.", sellerId);

				seller.ContactStatusId = 5;
				unitOfWork.Save();
			}
			
		}

	}

	private async Task TriggerExternalCall(Contact seller)
	{
		var client = _httpClientFactory.CreateClient();

		var requestBody = new
		{
			callType = "resales",
			leadInfo = new
			{
				id = seller.ContactId,
				name = seller.Name,
				phone = seller.Phone
			}
		};

		var response = await client.PostAsJsonAsync("https://call-handler.vercel.app/api/v1/call", requestBody);

		// This ensures an exception is thrown if the status code is not 200-299
		response.EnsureSuccessStatusCode();
	}
}
