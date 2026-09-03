using MyBackend.Domain.Entities;

namespace MyBackend.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);

        Task<List<User>> GetAllUsersAsync();

        Task<User?> GetUserByIdAsync(int id);

        Task<bool> SetDeletedFlagAsync(int id, int deletedFlag);

        Task<bool> HasPermissionAsync(int userId, params string[] permissionKeys);

        Task<List<string>> GetUserPermissionKeysAsync(int userId);

        Task<bool> UpdatePasswordHashAsync(int userId, string newPasswordHash);
    }
}
