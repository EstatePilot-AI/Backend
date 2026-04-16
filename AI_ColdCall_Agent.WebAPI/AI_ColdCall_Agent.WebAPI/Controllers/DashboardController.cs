using AI_ColdCall_Agent.Core.DTO;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace AI_ColdCall_Agent.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : Controller
    {
		private readonly DashboardAnalyticsService _analyticsService;

		public DashboardController(DashboardAnalyticsService analyticsService)
        {
			_analyticsService = analyticsService;
		}

		[Authorize]
        [HttpGet("GetGlobalAnalytics")]
        public async Task<IActionResult> GetGlobalAnalytics(int? day, int? month, int? year, string? outcomeName = null)
        {

			var result = await _analyticsService.BuildAnalyticsAsync(
			 day, month, year, outcomeName);

			if (result == null)
				return Ok(new
				{
					message = "No data available for the selected filters.",
					generatedAt = DateTime.Now
				});

			return Ok(result);
		}
    }
}
