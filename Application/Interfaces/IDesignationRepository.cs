using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IDesignationRepository : IRepository<DesignationModel>
    {
        Task<List<DesignationModel>> GetActiveDesignationsAsync();

        Task<DesignationModel?> GetActiveDesignationByIdAsync(int id);

        Task<Dictionary<int, string>> GetDesignationNameDictionaryAsync();

        Task<bool> DesignationExistsByNameAsync(string name, int? excludeId = null);

        Task<string?> GetDepartmentNameByIdAsync(int departmentId);

        Task<bool> SetDeletedFlagAsync(int id, int deletedFlag);

        Task<List<DesignationModel>> GetDesignationsByIdsAsync(IEnumerable<int> ids);

        Task<List<DesignationModel>> GetDesignationsByDepartmentIdAsync(int departmentId);
    }
}
