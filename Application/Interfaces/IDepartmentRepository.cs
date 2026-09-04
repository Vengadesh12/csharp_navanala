using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        Task<List<Department>> GetActiveDepartmentsWithDesignationsAsync();

        Task<Department?> GetActiveDepartmentByIdAsync(int id);

        Task<Dictionary<int, string>> GetDepartmentNameDictionaryAsync();

        Task<bool> DepartmentExistsByNameAsync(string name, int? excludeId = null);
    }
}
