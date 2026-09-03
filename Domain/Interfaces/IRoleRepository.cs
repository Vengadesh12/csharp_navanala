using MyBackend.Domain.Entities;

namespace MyBackend.Domain.Interfaces
{
    public interface IRoleRepository : IRepository<Role>
    {
        Task<List<Role>> GetActiveRolesAsync();

        Task<Role?> GetActiveRoleByIdAsync(int id);

        Task<bool> SetDeletedFlagAsync(int id, int deletedFlag);

        Task<Dictionary<int, string>> GetRoleNameDictionaryAsync();
    }
}
