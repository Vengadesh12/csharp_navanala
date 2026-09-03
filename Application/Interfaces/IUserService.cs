using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();

        Task<UserDto?> GetUserByIdAsync(int id);

        Task<UserDto> CreateUserAsync(CreateUserRequest request);

        Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request);

        Task<bool> SoftDeleteUserAsync(int id);

        Task<bool> RestoreUserAsync(int id);

        Task<bool> HasPermissionAsync(int userId, params string[] permissionKeys);
    }
}
