using AI_ColdCall_Agent.Core.DTO;

namespace IServices
{
    /// <summary>
    /// Provides aggregated analytics data for the global dashboard.
    /// </summary>
    public interface IDashboardAnalyticsService
    {
        /// <summary>
        /// Builds a complete analytics snapshot based on the supplied filter criteria.
        /// </summary>
        /// <param name="request">Optional date-range and outcome filters.</param>
        /// <returns>
        /// A fully populated <see cref="DashboardAnalyticsResponse"/>, or <c>null</c>
        /// when no call logs match the filters.
        /// </returns>
        Task<DashboardAnalyticsResponse?> BuildAnalyticsAsync(GlobalAnalyticsRequest request);
    }
}
