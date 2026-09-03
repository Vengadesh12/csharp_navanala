using MyBackend.Application.Common.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IReportCategoryService
    {
        Task<List<ReportCategoryDto>> GetAllCategoriesAsync();
        Task<ReportCategoryDto?> GetCategoryByIdAsync(int id);
        Task<ReportCategoryDto> CreateCategoryAsync(CreateReportCategoryRequest request);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
