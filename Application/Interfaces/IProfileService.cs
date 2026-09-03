using Microsoft.AspNetCore.Http;
using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IProfileService
    {
        Task<UserProfileResponse> GetProfileAsync(int userId);
        Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<UserProfileResponse> UploadProfileImageAsync(int userId, IFormFile file);
        Task<UserProfileResponse> RemoveProfileImageAsync(int userId);
    }
}
