using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IProjectCategoryService
    {
        Task<List<ProjectCategoryDto>> GetAllCategoriesAsync();
        Task<ProjectCategoryDto?> GetCategoryByIdAsync(int id);
        Task<ProjectCategoryDto> CreateCategoryAsync(CreateProjectCategoryRequest request);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
