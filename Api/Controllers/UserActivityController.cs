using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using System.Security.Claims;

namespace MyBackend.Api.Controllers
{
    /// <summary>
    /// Live user activity monitoring, active online sessions, and login/logout audit trail.
    /// </summary>
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

        /// <summary>
        /// Retrieves high-level dashboard metrics, live active user sessions, and recent logins.
        /// </summary>
        /// <response code="200">User activity metrics and active sessions returned.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="403">Forbidden: Caller lacks 'user_activity.view' permission.</response>
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

        /// <summary>
        /// Retrieves paginated list of user login/logout activity sessions with optional search and status filter.
        /// </summary>
        /// <response code="200">Paginated list of activity records returned.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="403">Forbidden: Caller lacks 'user_activity.view' permission.</response>
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

        /// <summary>
        /// Retrieves list of currently logged in / active users.
        /// </summary>
        /// <response code="200">List of currently active sessions returned.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="403">Forbidden: Caller lacks 'user_activity.view' permission.</response>
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

        /// <summary>
        /// Force terminate an active session by session ID.
        /// </summary>
        /// <param name="sessionId">The ID of the session to terminate.</param>
        /// <response code="200">Session terminated successfully.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="403">Forbidden: Caller lacks manage permission.</response>
        /// <response code="404">Session not found.</response>
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

        /// <summary>
        /// Force logout a user from all their active sessions.
        /// </summary>
        /// <param name="userId">The target user ID to log out.</param>
        /// <response code="200">User sessions terminated.</response>
        /// <response code="401">Unauthorized: Authentication token is required.</response>
        /// <response code="403">Forbidden: Caller lacks manage permission.</response>
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
