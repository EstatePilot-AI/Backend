using AI_ColdCall_Agent.Core.DTO;
using Interfaces;
using IServices;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Services;

/// <inheritdoc cref="IServices.IDashboardAnalyticsService"/>
public class DashboardAnalyticsService : IDashboardAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardAnalyticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<DashboardAnalyticsResponse?> BuildAnalyticsAsync(
        GlobalAnalyticsRequest request)
    {
		DateTime? fromDate = request.FromDate?.ToUniversalTime();
		DateTime? toDate = request.ToDate?.ToUniversalTime();

		if ((fromDate > toDate) || (toDate.HasValue && toDate.Value > DateTime.UtcNow))
		{
			return new DashboardAnalyticsResponse();
		}

		var logs = await _unitOfWork.CallLogs.FindAllAsync(c =>
            (!request.FromDate.HasValue || c.Timestamp >= fromDate.Value) &&
            (!request.ToDate.HasValue || c.Timestamp <= toDate.Value) &&
            (string.IsNullOrEmpty(request.OutcomeName.ToString()) ||
             c.CallOutcome.Name.Replace(" ", "").ToLower() == request.OutcomeName.ToString().ToLower()),
            new string[] { "CallOutcome" }
        );

        var properties = await _unitOfWork.Properties
            .FindAllAsync(p => true, new string[] { "PropertyStatus" });

        if (logs == null || !logs.Any()) return null;

        int totalEngagements = logs.Count();
        int totalProperties = properties.Count();

        var outcomeDistributions = logs
            .GroupBy(l => l.CallOutcome?.Name ?? "Other")
            .Select(g => new OutcomeDistributionDto
            {
                CategoryName = g.Key,
                Occurrences = g.Count(),
                SharePercentage = Math.Round((double)g.Count() / totalEngagements * 100, 1)
            }).ToList();

        var hourlyTrends = logs
            .GroupBy(l => l.Timestamp.Hour)
            .OrderBy(g => g.Key)
            .Select(g => new PeakActivityDto
            {
                TimeSlot = DateTime.Today.AddHours(g.Key).ToString("hh:00 tt"),
                Volume = g.Count()
            }).ToList();

        var propertyInventory = properties
            .GroupBy(p => p.PropertyStatus?.Name ?? "Unknown")
            .Select(g => new PropertyStatusDto
            {
                StatusName = g.Key,
                Count = g.Count(),
                Percentage = totalProperties > 0
                    ? Math.Round((double)g.Count() / totalProperties * 100, 1) : 0
            })
            .OrderByDescending(x => x.Count)
            .ToList();

		// Calculate revenue and deal count for successful deals (assuming DealStatusId == 2 indicates completed)
		var successfulDeals = await _unitOfWork.Deals.FindAllAsync(d => d.DealStatusId == 2);
        var totalRevenue = successfulDeals.Sum(d => d.FinalSaleAmount);
        var dealCount= successfulDeals.Count();


		return new DashboardAnalyticsResponse
        {
            KeyMetrics = new KeyMetricsDto
            {
                TotalEngagements = totalEngagements,
                QualifiedLeads = logs.Count(l =>
                    l.CallOutcome?.Name?.ToLower() == "interested"),
                CallConversionRate = Math.Round(
                    (double)logs.Count(l => l.CallOutcome?.Name?.ToLower() == "interested")
                    / totalEngagements * 100, 1),
                ResourceOptimizationHours = Math.Round(
                    (double)logs.Sum(l => l.Duration) / 60, 1),
                AverageHandlingTime = Math.Round(
                    (double)logs.Average(l => l.Duration), 2),

                TotalRevenue = totalRevenue,
                AverageDealValue = dealCount > 0 ? Math.Round(totalRevenue / dealCount, 2) : 0,
                DealConversionRate = Math.Round((double)dealCount / logs.Count() * 100, 1)

			},
            OutcomeDistributions = outcomeDistributions,
            PeakActivityTrends = hourlyTrends,
            PropertyInventoryStatus = propertyInventory,
            GeneratedAt = DateTime.UtcNow
        };
    }
}
