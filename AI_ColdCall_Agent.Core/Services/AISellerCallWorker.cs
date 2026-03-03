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

	public AISellerCallWorker(IBackgroundTaskQueue queue, IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory)
	{
		_queue = queue;
		_scopeFactory = scopeFactory;
		_httpClientFactory = httpClientFactory;
	}
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			// Wait for the signal from CSV upload or previous call outcome
			var sellerId = await _queue.DequeueAsync(stoppingToken);

			await ExecuteSellerCallLogic(sellerId);

			// Safety delay to prevent overlapping calls
			await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
		}
	}

	private async Task ExecuteSellerCallLogic(int sellerId)
	{
		using var scope = _scopeFactory.CreateScope();
		var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

		//Get seller contact
		var seller = unitOfWork.Contacts.FindOneItem(c => c.ContactId == sellerId && c.ContactTypeId==2); //2 refers to seller contact type

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
			seller.ContactStatusId = 2; //qualified - call in progress
			unitOfWork.Save();

			await TriggerExternalCall(seller);
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

		await client.PostAsJsonAsync("https://call-handler.vercel.app/api/v1/call", requestBody);
	}
}
