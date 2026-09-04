using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    public interface IDesignationRepository : IRepository<Designation>
    {
        Task<List<Designation>> GetActiveDesignationsAsync();

        Task<Designation?> GetActiveDesignationByIdAsync(int id);

        Task<Dictionary<int, string>> GetDesignationNameDictionaryAsync();

        Task<bool> DesignationExistsByNameAsync(string name, int? excludeId = null);

        Task<string?> GetDepartmentNameByIdAsync(int departmentId);

        Task<bool> SetDeletedFlagAsync(int id, int deletedFlag);

        Task<List<Designation>> GetDesignationsByIdsAsync(IEnumerable<int> ids);

        Task<List<Designation>> GetDesignationsByDepartmentIdAsync(int departmentId);
    }
}
