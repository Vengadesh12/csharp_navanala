using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardSummaryResponse> GetDashboardSummaryAsync(string? timeframe = "30d");
    }
}
