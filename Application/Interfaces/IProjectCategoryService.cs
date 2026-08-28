using MyBackend.Application.Contracts;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Service contract for managing project categories in PostgreSQL.
    /// </summary>
    public interface IProjectCategoryService
    {
        Task<List<ProjectCategoryDto>> GetAllCategoriesAsync();
        Task<ProjectCategoryDto?> GetCategoryByIdAsync(int id);
        Task<ProjectCategoryDto> CreateCategoryAsync(CreateProjectCategoryRequest request);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
