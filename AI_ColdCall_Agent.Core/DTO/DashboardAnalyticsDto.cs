using System;
using System.Collections.Generic;
using System.Text;

namespace AI_ColdCall_Agent.Core.DTO
{
    public class DashboardAnalyticsDto
    {
        public KeyMetricsDto KeyMetrics { get; set; }
        public List<OutcomeDistributionDto> OutcomeDistributions { get; set; }
        public List<PeakActivityDto> PeakActivityTrends { get; set; }
        public List<PropertyStatusDto> PropertyInventoryStatus { get; set; }
        public DateTime GeneratedAt { get; set; }

    }

    public class KeyMetricsDto
    {
        public int TotalEngagements { get; set; }
        public int QualifiedLeads { get; set; } // "Interested" count
        public double ConversionRate { get; set; } // Success %
        public double ResourceOptimizationHours { get; set; } // Time saved
        public double AverageHandlingTime { get; set; } // AHT
    }

    // Pie Chart data
    public class OutcomeDistributionDto
    {
        public string CategoryName { get; set; }
        public int Occurrences { get; set; }
        public double SharePercentage { get; set; }
    }

    // Line/Bar Chart data for call volume
    public class PeakActivityDto
    {
        public string TimeSlot { get; set; } // e.g., "10:00 AM"
        public int Volume { get; set; }
    }

    public class PropertyStatusDto
    {
        public string StatusName { get; set; } // e.g., Available, Sold
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
