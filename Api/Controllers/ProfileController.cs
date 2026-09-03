using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Tags("Profile")]
    [Produces("application/json")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { Message = "Valid user authentication token required." });
            }

            var profile = await _profileService.GetProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { Message = "Valid user authentication token required." });
            }

            var updatedProfile = await _profileService.UpdateProfileAsync(userId, request);

            return Ok(new ApiResponse<UserProfileResponse>
            {
                Success = true,
                Message = "Profile updated in database successfully!",
                Data = updatedProfile
            });
        }

        [HttpPut("change-password")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { Message = "Valid user authentication token required." });
            }

            await _profileService.ChangePasswordAsync(userId, request);

            return Ok(new MessageResponse
            {
                Success = true,
                Message = "Password changed succesfully!"
            });
        }
        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { Message = "Valid user authentication token required." });
            }

            var updatedProfile = await _profileService.UploadProfileImageAsync(userId, file);

            return Ok(new ApiResponse<UserProfileResponse>
            {
                Success = true,
                Message = "Profile picture uploaded successfully!",
                Data = updatedProfile
            });
        }

        [HttpDelete("remove-image")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveProfileImage()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized(new ErrorResponse { Message = "Valid user authentication token required." });
            }

            var updatedProfile = await _profileService.RemoveProfileImageAsync(userId);

            return Ok(new ApiResponse<UserProfileResponse>
            {
                Success = true,
                Message = "Profile picture removed successfully!",
                Data = updatedProfile
            });
        }
    }
}
