using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AI_ColdCall_Agent.Core.DTO
{
    /// <summary>
    /// Aggregated global analytics response for the dashboard.
    /// Contains key performance metrics, call outcome distribution,
    /// peak activity trends, and property inventory breakdown.
    /// </summary>
    public class DashboardAnalyticsResponse
    {
        /// <summary>
        /// High-level KPI summary (total engagements, leads, conversion rate, etc.).
        /// </summary>
        [JsonPropertyName("keyMetrics")]
        public KeyMetricsDto KeyMetrics { get; set; }

        /// <summary>
        /// Breakdown of call outcomes by category with occurrence counts and share percentages.
        /// Suitable for pie/donut chart rendering.
        /// </summary>
        [JsonPropertyName("outcomeDistributions")]
        public List<OutcomeDistributionDto> OutcomeDistributions { get; set; }

        /// <summary>
        /// Hourly call volume distribution across the filtered period.
        /// Suitable for line/bar chart rendering.
        /// </summary>
        [JsonPropertyName("peakActivityTrends")]
        public List<PeakActivityDto> PeakActivityTrends { get; set; }

        /// <summary>
        /// Property inventory grouped by current status (e.g. Available, Sold).
        /// </summary>
        [JsonPropertyName("propertyInventoryStatus")]
        public List<PropertyStatusDto> PropertyInventoryStatus { get; set; }

        /// <summary>
        /// UTC timestamp indicating when this analytics snapshot was generated.
        /// </summary>
        /// <example>2026-04-17T14:30:00Z</example>
        [JsonPropertyName("generatedAt")]
        public DateTime GeneratedAt { get; set; }
    }

    /// <summary>
    /// Key performance indicators for the dashboard.
    /// </summary>
    public class KeyMetricsDto
    {
        /// <summary>
        /// Total number of call engagements matching the applied filters.
        /// </summary>
        /// <example>1248</example>
        [JsonPropertyName("totalEngagements")]
        public int TotalEngagements { get; set; }

        /// <summary>
        /// Number of calls that resulted in an "Interested" outcome (qualified leads).
        /// </summary>
        /// <example>342</example>
        [JsonPropertyName("qualifiedLeads")]
        public int QualifiedLeads { get; set; }

        /// <summary>
        /// Percentage of engagements that converted to qualified leads.
        /// Range: 0.0 – 100.0.
        /// </summary>
        /// <example>27.4</example>
        [JsonPropertyName("conversionRate")]
        public double ConversionRate { get; set; }

        /// <summary>
        /// Estimated hours saved through AI-driven automation (total call duration in hours).
        /// </summary>
        /// <example>186.5</example>
        [JsonPropertyName("resourceOptimizationHours")]
        public double ResourceOptimizationHours { get; set; }

        /// <summary>
        /// Average handling time per call in seconds.
        /// </summary>
        /// <example>127.45</example>
        [JsonPropertyName("averageHandlingTime")]
        public double AverageHandlingTime { get; set; }
    }

    /// <summary>
    /// A single slice in the call outcome distribution (pie/donut chart).
    /// </summary>
    public class OutcomeDistributionDto
    {
        /// <summary>
        /// Human-readable name of the call outcome category (e.g. "Interested", "NotAnswer").
        /// </summary>
        /// <example>Interested</example>
        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; }

        /// <summary>
        /// Absolute count of calls in this category.
        /// </summary>
        /// <example>342</example>
        [JsonPropertyName("occurrences")]
        public int Occurrences { get; set; }

        /// <summary>
        /// Percentage of total engagements this category represents.
        /// Range: 0.0 – 100.0.
        /// </summary>
        /// <example>27.4</example>
        [JsonPropertyName("sharePercentage")]
        public double SharePercentage { get; set; }
    }

    /// <summary>
    /// A single data point in the hourly call volume trend (line/bar chart).
    /// </summary>
    public class PeakActivityDto
    {
        /// <summary>
        /// Time slot label in 12-hour format (e.g. "09:00 AM").
        /// </summary>
        /// <example>10:00 AM</example>
        [JsonPropertyName("timeSlot")]
        public string TimeSlot { get; set; }

        /// <summary>
        /// Number of calls that occurred during this hour.
        /// </summary>
        /// <example>87</example>
        [JsonPropertyName("volume")]
        public int Volume { get; set; }
    }

    /// <summary>
    /// A single row in the property inventory status breakdown.
    /// </summary>
    public class PropertyStatusDto
    {
        /// <summary>
        /// Human-readable name of the property status (e.g. "Available", "Sold", "Reserved").
        /// </summary>
        /// <example>Available</example>
        [JsonPropertyName("statusName")]
        public string StatusName { get; set; }

        /// <summary>
        /// Number of properties currently in this status.
        /// </summary>
        /// <example>156</example>
        [JsonPropertyName("count")]
        public int Count { get; set; }

        /// <summary>
        /// Percentage of total properties this status represents.
        /// Range: 0.0 – 100.0.
        /// </summary>
        /// <example>62.4</example>
        [JsonPropertyName("percentage")]
        public double Percentage { get; set; }
    }
}
