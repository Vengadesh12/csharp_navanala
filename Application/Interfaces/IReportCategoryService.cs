using MyBackend.Application.Contracts;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Service contract for managing report categories in PostgreSQL.
    /// </summary>
    public interface IReportCategoryService
    {
        Task<List<ReportCategoryDto>> GetAllCategoriesAsync();
        Task<ReportCategoryDto?> GetCategoryByIdAsync(int id);
        Task<ReportCategoryDto> CreateCategoryAsync(CreateReportCategoryRequest request);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
