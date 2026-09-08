using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IReportRepository
    {
        Task<(List<ReportModel> Reports, int TotalReports, int ReadyReports, int TotalUsers, int UsersWithRole, List<ReportCategoryModel> Categories)> GetReportsOverviewDataAsync(string? category, string? search, CancellationToken cancellationToken = default);

        Task<List<string>> GetCategoryNamesAsync(CancellationToken cancellationToken = default);

        Task<ReportModel?> GetReportByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<ReportModel> AddReportAsync(ReportModel report, CancellationToken cancellationToken = default);

        Task<ReportModel> CreateReportRecordAsync(string title, string description, int? categoryId, string categoryName, string format, string creatorName, string fileSize, string? storedFileName, CancellationToken cancellationToken = default);

        Task<ReportModel?> UpdateReportRecordAsync(int id, string title, string description, int? categoryId, string categoryName, string format, string? status, string? newFileName, string? newFileSize, CancellationToken cancellationToken = default);

        Task UpdateReportAsync(ReportModel report, CancellationToken cancellationToken = default);

        Task<bool> SoftDeleteReportAsync(int id, CancellationToken cancellationToken = default);

        Task<List<ReportCategoryModel>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);

        Task<ReportCategoryModel?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<ReportCategoryModel?> GetCategoryByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<bool> CategoryExistsByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<ReportCategoryModel> AddCategoryAsync(ReportCategoryModel category, CancellationToken cancellationToken = default);

        Task<bool> SoftDeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
    }
}
