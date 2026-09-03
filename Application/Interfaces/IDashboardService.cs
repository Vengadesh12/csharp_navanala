using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Service contract for aggregate workspace dashboard metrics, charts, and activity audit items.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Retrieves aggregated KPI metrics, role distribution charts, and recent activity items.
        /// </summary>
        /// <param name="timeframe">Optional timeframe filter (e.g. 7d, 30d, 90d, 12m).</param>
        Task<DashboardSummaryResponse> GetDashboardSummaryAsync(string? timeframe = "30d");
    }
}
