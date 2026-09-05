using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IReportService
    {
        Task<ReportsOverviewResponse> GetReportsAsync(string? category, string? search, CancellationToken cancellationToken = default);
        Task<List<string>> GetCategoriesAsync(CancellationToken cancellationToken = default);
        Task<ReportDownloadResult?> GetReportDownloadAsync(int id, CancellationToken cancellationToken = default);
        Task<ReportDto> CreateReportAsync(CreateReportRequest request, string creatorName, CancellationToken cancellationToken = default);
        Task<ReportDto?> UpdateReportAsync(int id, UpdateReportRequest request, CancellationToken cancellationToken = default);
        Task<bool> DeleteReportAsync(int id, CancellationToken cancellationToken = default);
    }
}
