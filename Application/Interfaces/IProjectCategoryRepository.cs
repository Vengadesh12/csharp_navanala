using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IProjectCategoryRepository
    {
        Task<List<ProjectCategoryModel>> GetAllCategoriesAsync();

        Task<ProjectCategoryModel?> GetCategoryByIdAsync(int id);

        Task<bool> CategoryExistsByNameAsync(string name);

        Task<ProjectCategoryModel> AddCategoryAsync(ProjectCategoryModel category);

        Task<bool> SoftDeleteCategoryAsync(int id);
    }
}
