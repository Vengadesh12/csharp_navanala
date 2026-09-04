using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    public interface IProjectCategoryRepository
    {
        Task<List<ProjectCategory>> GetAllCategoriesAsync();

        Task<ProjectCategory?> GetCategoryByIdAsync(int id);

        Task<bool> CategoryExistsByNameAsync(string name);

        Task<ProjectCategory> AddCategoryAsync(ProjectCategory category);

        Task<bool> SoftDeleteCategoryAsync(int id);
    }
}
