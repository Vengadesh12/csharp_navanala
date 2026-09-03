using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    /// <summary>
    /// Administrative dashboard statistics, aggregations, and visual chart data endpoints.
    /// </summary>
    [ApiController]
    [Route("api/dashboard")]
    [Tags("Dashboard")]
    [Produces("application/json")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Retrieve aggregated real-time dashboard metrics, charts, role distribution, and recent users from database.
        /// </summary>
        /// <param name="timeframe">Timeframe filter ("7d", "30d", "90d"). Defaults to "7d".</param>
        /// <response code="200">Dashboard metrics aggregated and returned successfully.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(DashboardSummaryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDashboardSummary([FromQuery] string timeframe = "7d")
        {
            var response = await _dashboardService.GetDashboardSummaryAsync(timeframe);
            return Ok(response);
        }
    }
}
