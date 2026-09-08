using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IUserRepository : IRepository<UserModel>
    {
        Task<UserModel?> GetByEmailAsync(string email);

        Task<UserLoginDetailsModel?> GetLoginUserDetailsByEmailAsync(string email);

        Task<List<UserModel>> GetAllUsersAsync();

        Task<UserModel?> GetUserByIdAsync(int id);

        Task<bool> SetDeletedFlagAsync(int id, int deletedFlag);

        Task<bool> HasPermissionAsync(int userId, params string[] permissionKeys);

        Task<List<string>> GetUserPermissionKeysAsync(int userId, int? roleId = null, int? designationId = null);

        Task<bool> UpdatePasswordHashAsync(int userId, string newPasswordHash);

        Task<Dictionary<int, string>> GetActiveRolesLookupAsync();

        Task<Dictionary<int, string>> GetActiveDesignationsLookupAsync();

        Task<string?> GetRoleNameByIdAsync(int roleId);

        Task<string?> GetDesignationNameByIdAsync(int designationId);

        Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);

        Task<bool> PhoneExistsAsync(string phone, int? excludeUserId = null);

        Task<int> GetActiveUsersCountAsync();

        Task<int> GetUsersWithRoleCountAsync();

        Task<List<string>> GetUserPermissionKeysForProfileAsync(int roleId, int designationId);

        Task<Dictionary<int, string>> GetUserRoleMapAsync();
    }
}
