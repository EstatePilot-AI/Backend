using System.ComponentModel.DataAnnotations;

namespace AI_ColdCall_Agent.Core.DTO
{
    /// <summary>
    /// Filter parameters for global dashboard analytics.
    /// All fields are optional — omit to get all-time aggregated statistics.
    /// </summary>
    public class GlobalAnalyticsRequest
    {
        /// <summary>
        /// Start of the date range (inclusive). Defaults to earliest record if omitted.
        /// </summary>
        /// <example>2026-01-01</example>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// End of the date range (inclusive). Defaults to today if omitted.
        /// </summary>
        /// <example>2026-04-17</example>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Filter by call outcome name (e.g. "Interested", "NotInterested", "NotAnswer", "Failed").
        /// Case-insensitive match. Omit to include all outcomes.
        /// </summary>
        /// <example>Interested</example>
        [MaxLength(100)]
        public string? OutcomeName { get; set; }
    }
}
