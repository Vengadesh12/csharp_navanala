using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using System.Security.Claims;

namespace MyBackend.Api.Controllers
{
    [ApiController]
    [Route("api/user-activity")]
    [Tags("UserActivity")]
    [Produces("application/json")]
    [Authorize]
    public class UserActivityController : ControllerBase
    {
        private readonly IUserActivityService _userActivityService;
        private readonly IUserService _userService;

        public UserActivityController(IUserActivityService userActivityService, IUserService userService)
        {
            _userActivityService = userActivityService;
            _userService = userService;
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(UserActivitySummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSummary()
        {
            if (!await HasViewPermissionAsync())
            {
                return Forbid();
            }

            var summary = await _userActivityService.GetSummaryAsync();
            return Ok(summary);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedUserActivityResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetActivities([FromQuery] UserActivityQueryParameters query)
        {
            if (!await HasViewPermissionAsync())
            {
                return Forbid();
            }

            var activities = await _userActivityService.GetPagedActivitiesAsync(query);
            return Ok(activities);
        }

        [HttpGet("active")]
        [ProducesResponseType(typeof(List<UserSessionItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetActiveUsers()
        {
            if (!await HasViewPermissionAsync())
            {
                return Forbid();
            }

            var activeUsers = await _userActivityService.GetActiveUsersAsync();
            return Ok(activeUsers);
        }

        [HttpPost("terminate/{sessionId:int}")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TerminateSession(int sessionId)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminUserId))
            {
                return Unauthorized(new ErrorResponse { Message = "A valid user token is required." });
            }

            if (!await HasManagePermissionAsync(adminUserId))
            {
                return Forbid();
            }

            var session = await _userActivityService.GetSessionByIdAsync(sessionId);
            if (session == null)
            {
                return NotFound(new ErrorResponse { Message = $"Active session #{sessionId} was not found." });
            }

            var success = await _userActivityService.TerminateSessionAsync(sessionId, adminUserId);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Active session #{sessionId} was not found." });
            }

            return Ok(new MessageResponse { Success = true, Message = "Session terminated successfully." });
        }

        [HttpPost("force-logout/{userId:int}")]
        [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ForceLogoutUser(int userId)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminUserId))
            {
                return Unauthorized(new ErrorResponse { Message = "A valid user token is required." });
            }

            if (!await HasManagePermissionAsync(adminUserId))
            {
                return Forbid();
            }

            var count = await _userActivityService.ForceLogoutUserAsync(userId, adminUserId);
            return Ok(new MessageResponse
            {
                Success = true,
                Message = count > 0 ? $"{count} active session(s) terminated for user." : "No active sessions found for user."
            });
        }

        private async Task<bool> HasViewPermissionAsync()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) || userId <= 0)
            {
                return false;
            }

            return await _userService.HasPermissionAsync(userId,
                "user_activity.view",
                "user_activity.force_logout",
                "user_activity.manage",
                "audit.view",
                "dashboard.view",
                "users.view",
                "settings.view");
        }

        private async Task<bool> HasManagePermissionAsync(int userId)
        {
            if (userId <= 0) return false;

            return await _userService.HasPermissionAsync(userId,
                "user_activity.force_logout",
                "permissions.manage");
        }
    }
}
