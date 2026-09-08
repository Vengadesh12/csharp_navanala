using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IRoleRepository : IRepository<RoleModel>
    {
        Task<List<RoleModel>> GetActiveRolesAsync();

        Task<RoleModel?> GetActiveRoleByIdAsync(int id);

        Task<bool> SetDeletedFlagAsync(int id, int deletedFlag);

        Task<Dictionary<int, string>> GetRoleNameDictionaryAsync();
    }
}
