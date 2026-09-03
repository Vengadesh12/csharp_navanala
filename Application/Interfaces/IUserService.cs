using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    /// <summary>
    /// Service contract for user lifecycle management, role assignments, and permission authorization.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves all user accounts formatted as UserDto.
        /// </summary>
        Task<List<UserDto>> GetAllUsersAsync();

        /// <summary>
        /// Retrieves detailed information for a single user by their identifier.
        /// </summary>
        Task<UserDto?> GetUserByIdAsync(int id);

        /// <summary>
        /// Provisions a new user account with hashed password and sends welcome credentials email.
        /// </summary>
        Task<UserDto> CreateUserAsync(CreateUserRequest request);

        /// <summary>
        /// Updates an existing user's profile and credentials.
        /// </summary>
        Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request);

        /// <summary>
        /// Soft-deletes / deactivates a user account.
        /// </summary>
        Task<bool> SoftDeleteUserAsync(int id);

        /// <summary>
        /// Restores a soft-deleted user account.
        /// </summary>
        Task<bool> RestoreUserAsync(int id);

        /// <summary>
        /// Checks whether the specified user has any of the required permission keys.
        /// </summary>
        Task<bool> HasPermissionAsync(int userId, params string[] permissionKeys);
    }
}
