using AI_ColdCall_Agent.Core.DTO;
using IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AI_ColdCall_Agent.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardAnalyticsService _analyticsService;

        public DashboardController(IDashboardAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Retrieves aggregated global analytics for the dashboard, including key metrics,
        /// call outcome distribution, peak activity trends, and property inventory status.
        /// </summary>
        /// <remarks>
        /// All filter parameters are optional. When no filters are provided, the endpoint
        /// returns all-time aggregated statistics.
        ///
        /// **Date range:** Provide <c>fromDate</c> and/or <c>toDate</c> to scope the analytics
        /// to a specific time window. Dates are compared against the call timestamp (inclusive).
        ///
        /// **Outcome filter:** Use <c>outcomeName</c> to narrow results to a specific call outcome
        /// (e.g. "Interested", "NotInterested", "NotAnswer", "Failed"). The match is case-insensitive.
        ///
        /// **Example request:**
        ///     GET /api/Dashboard/GetGlobalAnalytics?fromDate=2026-01-01&amp;toDate=2026-04-17&amp;outcomeName=Interested
        /// </remarks>
        /// <param name="request">Optional filter parameters for date range and call outcome.</param>
        /// <returns>Aggregated analytics data, or 204 if no matching records found.</returns>
        /// <response code="200">Returns the aggregated analytics payload.</response>
        /// <response code="204">No data available for the selected filters.</response>
        /// <response code="401">Unauthorized — valid JWT token required.</response>
        /// <response code="403">Forbidden — requires the <c>superadmin</c> role.</response>
        [Authorize(Roles = "superadmin")]
        [HttpGet("GetGlobalAnalytics")]
        [ProducesResponseType(typeof(DashboardAnalyticsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<DashboardAnalyticsResponse>> GetGlobalAnalytics(
            [FromQuery] GlobalAnalyticsRequest request)
        {
            var result = await _analyticsService.BuildAnalyticsAsync(request);

            if (result == null)
                return NoContent();

            return Ok(result);
        }
    }
}
