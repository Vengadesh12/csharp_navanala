using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Contracts;
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

        /// <summary>
        /// Retrieve the profile of the currently logged-in user.
        /// </summary>
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

        /// <summary>
        /// Update profile details for the currently logged-in user.
        /// </summary>
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

        /// <summary>
        /// Change account password for the currently logged-in user.
        /// </summary>
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

        /// <summary>
        /// Upload a new profile picture for the currently logged-in user.
        /// </summary>
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

        /// <summary>
        /// Remove profile picture for the currently logged-in user and revert to default.
        /// </summary>
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
