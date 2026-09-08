using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IDepartmentRepository : IRepository<DepartmentModel>
    {
        Task<List<DepartmentModel>> GetActiveDepartmentsWithDesignationsAsync();

        Task<DepartmentModel?> GetActiveDepartmentByIdAsync(int id);

        Task<Dictionary<int, string>> GetDepartmentNameDictionaryAsync();

        Task<bool> DepartmentExistsByNameAsync(string name, int? excludeId = null);
    }
}
