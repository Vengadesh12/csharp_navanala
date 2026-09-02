using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    /// <summary>
    /// Employee permission request management and Super Admin decision endpoints.
    /// </summary>
    [ApiController]
    [Route("api/access-requests")]
    [Tags("Access Requests")]
    [Produces("application/json")]
    [Authorize]
    public class AccessRequestsController : ControllerBase
    {
        private readonly IAccessRequestService _accessRequestService;
        private readonly IUserService _userService;

        public AccessRequestsController(
            IAccessRequestService accessRequestService,
            IUserService userService)
        {
            _accessRequestService = accessRequestService;
            _userService = userService;
        }

        /// <summary>
        /// Retrieves all system permissions categorized by module with grant status for the calling user.
        /// </summary>
        [HttpGet("available-permissions")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<AvailablePermissionDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailablePermissions()
        {
            var (userId, _, _) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            var permissions = await _accessRequestService.GetAvailablePermissionsAsync(userId);
            return Ok(new ApiResponse<System.Collections.Generic.List<AvailablePermissionDto>>
            {
                Success = true,
                Message = "Available permissions catalog retrieved.",
                Data = permissions
            });
        }

        /// <summary>
        /// Retrieves access requests submitted by the calling user.
        /// </summary>
        [HttpGet("my-requests")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<AccessRequestDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyRequests()
        {
            var (userId, _, _) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            var requests = await _accessRequestService.GetMyRequestsAsync(userId);
            return Ok(new ApiResponse<System.Collections.Generic.List<AccessRequestDto>>
            {
                Success = true,
                Message = "My access requests retrieved.",
                Data = requests
            });
        }

        /// <summary>
        /// Retrieves summary KPI metric counts for the access request dashboard.
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(AccessRequestSummaryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary()
        {
            var (userId, _, isSuperAdmin) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            var summary = await _accessRequestService.GetSummaryAsync(userId, isSuperAdmin);
            return Ok(summary);
        }

        /// <summary>
        /// Retrieves paginated access requests with status, priority, and search filters.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedAccessRequestResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRequests([FromQuery] AccessRequestQueryParameters query)
        {
            var (userId, _, isSuperAdmin) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            var response = await _accessRequestService.GetRequestsAsync(query, userId, isSuperAdmin);
            return Ok(response);
        }

        /// <summary>
        /// Retrieves a specific access request by its unique ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AccessRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetRequestById(int id)
        {
            var (userId, _, isSuperAdmin) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            var request = await _accessRequestService.GetRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound(new ErrorResponse { Message = $"Access request #{id} not found." });
            }

            if (!isSuperAdmin && request.UserId != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = "Access denied. You can only view your own access requests." });
            }

            return Ok(new ApiResponse<AccessRequestDto>
            {
                Success = true,
                Message = "Access request retrieved.",
                Data = request
            });
        }

        /// <summary>
        /// Submits a new permission access request.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AccessRequestDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRequest([FromBody] CreateAccessRequestDto dto)
        {
            var (userId, _, _) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            try
            {
                var created = await _accessRequestService.CreateRequestAsync(userId, dto);
                return StatusCode(StatusCodes.Status201Created, new ApiResponse<AccessRequestDto>
                {
                    Success = true,
                    Message = $"Access request for '{created.PermissionName}' submitted successfully for administrator review.",
                    Data = created
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Approves a pending permission request and immediately grants the permission to the user.
        /// </summary>
        [HttpPost("{id:int}/approve")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveRequest(int id, [FromBody] ReviewAccessRequestDto dto)
        {
            var (userId, userName, isSuperAdmin) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            if (!isSuperAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = "Only Super Administrators or managers with permission management authority can approve access requests." });
            }

            var success = await _accessRequestService.ApproveRequestAsync(id, userId, userName, dto);
            if (!success)
            {
                return BadRequest(new ErrorResponse { Message = $"Unable to approve access request #{id}. It may already be processed or deleted." });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = $"Access request #{id} approved and permission successfully assigned.",
                Data = true
            });
        }

        /// <summary>
        /// Rejects a pending permission request with reviewer feedback notes.
        /// </summary>
        [HttpPost("{id:int}/reject")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RejectRequest(int id, [FromBody] ReviewAccessRequestDto dto)
        {
            var (userId, userName, isSuperAdmin) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            if (!isSuperAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = "Only Super Administrators or managers with permission management authority can reject access requests." });
            }

            var success = await _accessRequestService.RejectRequestAsync(id, userId, userName, dto);
            if (!success)
            {
                return BadRequest(new ErrorResponse { Message = $"Unable to reject access request #{id}. It may already be processed or deleted." });
            }

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = $"Access request #{id} has been rejected.",
                Data = true
            });
        }

        /// <summary>
        /// Cancels or soft-deletes a pending access request.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRequest(int id)
        {
            var (userId, _, isSuperAdmin) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            var success = await _accessRequestService.DeleteRequestAsync(id, userId, isSuperAdmin);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Access request #{id} not found or cannot be cancelled." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Access request cancelled / removed successfully.",
                Id = id,
                DeletedFlag = 0
            });
        }

        private async Task<(int UserId, string UserName, bool IsSuperAdmin)> GetCallerContextAsync()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) || userId <= 0)
            {
                return (0, string.Empty, false);
            }

            var dbUser = await _userService.GetUserByIdAsync(userId);
            if (dbUser == null) return (userId, string.Empty, false);

            var roleId = dbUser.RoleId ?? 0;
            var roleName = (dbUser.RoleName ?? "").Trim().ToLowerInvariant();

            // Super Admin role ID = 2 or matching role title has full permission management
            bool isSuperAdmin = roleId == 2 || roleName.Contains("super admin") || roleName == "admin";

            return (userId, dbUser.Name, isSuperAdmin);
        }
    }
}
