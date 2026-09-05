using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    public interface IReportRepository
    {
        Task<(List<Report> Reports, int TotalReports, int ReadyReports, int TotalUsers, int UsersWithRole, List<ReportCategory> Categories)> GetReportsOverviewDataAsync(string? category, string? search, CancellationToken cancellationToken = default);

        Task<List<string>> GetCategoryNamesAsync(CancellationToken cancellationToken = default);

        Task<Report?> GetReportByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<Report> AddReportAsync(Report report, CancellationToken cancellationToken = default);

        Task<Report> CreateReportRecordAsync(string title, string description, int? categoryId, string categoryName, string format, string creatorName, string fileSize, string? storedFileName, CancellationToken cancellationToken = default);

        Task<Report?> UpdateReportRecordAsync(int id, string title, string description, int? categoryId, string categoryName, string format, string? status, string? newFileName, string? newFileSize, CancellationToken cancellationToken = default);

        Task UpdateReportAsync(Report report, CancellationToken cancellationToken = default);

        Task<bool> SoftDeleteReportAsync(int id, CancellationToken cancellationToken = default);

        Task<List<ReportCategory>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);

        Task<ReportCategory?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<ReportCategory?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<bool> CategoryExistsByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<ReportCategory> AddCategoryAsync(ReportCategory category, CancellationToken cancellationToken = default);

        Task<bool> SoftDeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
    }
}
