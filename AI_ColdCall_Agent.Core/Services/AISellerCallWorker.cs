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
		_logger.LogInformation("Seller Worker started in Polling Mode.");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = _scopeFactory.CreateScope();
				var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

				// 1. Find all potential candidates (Status 1 or 5)
				var candidates = await unitOfWork.Contacts.FindAllAsync(c =>
					c.ContactTypeId == 2 && (c.ContactStatusId == 1 || c.ContactStatusId == 5));

				Contact? nextSeller = null;

				foreach (var seller in candidates.OrderBy(c => c.ContactId))
				{
					if (seller.ContactStatusId == 1) // Fresh lead
					{
						nextSeller = seller;
						break;
					}

					if (seller.ContactStatusId == 5) // Retry lead
					{
						// Check the last call log for this specific seller
						var logs = await unitOfWork.CallLogs.FindAllAsync(cl => cl.ContactId == seller.ContactId);
						var lastCall = logs.OrderByDescending(cl => cl.Timestamp).FirstOrDefault();

						// ONLY proceed if 2 hours have passed since the LAST call
						if (lastCall == null || (DateTime.UtcNow - lastCall.Timestamp).TotalHours >= 2)
						{
							nextSeller = seller;
							break;
						}
					}
				}

				if (nextSeller != null)
				{
					_logger.LogInformation("Processing Seller {Id}. Status: {Status}", nextSeller.ContactId, nextSeller.ContactStatusId);

					// 2. LOCK immediately to Status 7 (Initiated)
					nextSeller.ContactStatusId = 7;
					unitOfWork.Save();

					await TriggerExternalCall(nextSeller);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Worker Loop Error");
			}

			// Wait 15-20 seconds before checking the DB again
			await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
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
