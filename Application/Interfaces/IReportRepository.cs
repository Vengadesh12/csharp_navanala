using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    public interface IReportRepository
    {
        Task<(List<Report> Reports, int TotalReports, int ReadyReports, int TotalUsers, int UsersWithRole, List<ReportCategory> Categories)> GetReportsOverviewDataAsync(string? category, string? search);

        Task<List<string>> GetCategoryNamesAsync();

        Task<Report?> GetReportByIdAsync(int id);

        Task<Report> AddReportAsync(Report report);

        Task<Report> CreateReportRecordAsync(string title, string description, int? categoryId, string categoryName, string format, string creatorName, string fileSize, string? storedFileName);

        Task<Report?> UpdateReportRecordAsync(int id, string title, string description, int? categoryId, string categoryName, string format, string? status, string? newFileName, string? newFileSize);

        Task UpdateReportAsync(Report report);

        Task<bool> SoftDeleteReportAsync(int id);

        Task<List<ReportCategory>> GetAllCategoriesAsync();

        Task<ReportCategory?> GetCategoryByIdAsync(int id);

        Task<ReportCategory?> GetCategoryByNameAsync(string name);

        Task<bool> CategoryExistsByNameAsync(string name);


        Task<ReportCategory> AddCategoryAsync(ReportCategory category);

        Task<bool> SoftDeleteCategoryAsync(int id);
    }
}
