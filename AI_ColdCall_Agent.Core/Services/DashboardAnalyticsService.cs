using AI_ColdCall_Agent.Core.DTO;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Services;

public class DashboardAnalyticsService
{
	
	private readonly IUnitOfWork _unitOfWork;

	public DashboardAnalyticsService(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<DashboardAnalyticsDto?> BuildAnalyticsAsync(
		int? day, int? month, int? year, string? outcomeName = null)
	{
		var logs = await _unitOfWork.CallLogs.FindAllAsync(c =>
			(!year.HasValue || c.Timestamp.Year == year) &&
			(!month.HasValue || c.Timestamp.Month == month) &&
			(!day.HasValue || c.Timestamp.Day == day) &&
			(string.IsNullOrEmpty(outcomeName) || c.CallOutcome.Name == outcomeName),
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

		return new DashboardAnalyticsDto
		{
			KeyMetrics = new KeyMetricsDto
			{
				TotalEngagements = totalEngagements,
				QualifiedLeads = logs.Count(l =>
					l.CallOutcome?.Name?.ToLower() == "interested"),
				ConversionRate = Math.Round(
					(double)logs.Count(l => l.CallOutcome?.Name?.ToLower() == "interested")
					/ totalEngagements * 100, 1),
				ResourceOptimizationHours = Math.Round(
					(double)logs.Sum(l => l.Duration) / 60, 1),
				AverageHandlingTime = Math.Round(
					(double)logs.Average(l => l.Duration), 2)
			},
			OutcomeDistributions = outcomeDistributions,
			PeakActivityTrends = hourlyTrends,
			PropertyInventoryStatus = propertyInventory,
			GeneratedAt = DateTime.Now
		};
	}
}

