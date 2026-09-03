using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IReportService
    {
        Task<ReportsOverviewResponse> GetReportsAsync(string? category, string? search);
        Task<List<string>> GetCategoriesAsync();
        Task<ReportDownloadResult?> GetReportDownloadAsync(int id);
        Task<ReportDto> CreateReportAsync(CreateReportRequest request, string creatorName);
        Task<ReportDto?> UpdateReportAsync(int id, UpdateReportRequest request);
        Task<bool> DeleteReportAsync(int id);
    }
}
