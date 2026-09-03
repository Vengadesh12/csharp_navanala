using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    [ApiController]
    [Route("api/approvals")]
    [Tags("Approvals")]
    [Produces("application/json")]
    [Authorize]
    public class ApprovalsController : ControllerBase
    {
        private readonly IApprovalService _approvalService;
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;

        public ApprovalsController(
            IApprovalService approvalService,
            IUserService userService,
            IDepartmentService departmentService)
        {
            _approvalService = approvalService;
            _userService = userService;
            _departmentService = departmentService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedApprovalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetApprovals([FromQuery] ApprovalQueryParameters query)
        {
            var (userId, isManagerOrAdmin) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            var response = await _approvalService.GetApprovalsAsync(query, userId, isManagerOrAdmin);
            return Ok(response);
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApprovalSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSummary()
        {
            var (userId, isManagerOrAdmin) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            var summary = await _approvalService.GetSummaryAsync(userId, isManagerOrAdmin);
            return Ok(summary);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ApprovalRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetApprovalById(int id)
        {
            var (userId, isManagerOrAdmin) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            var item = await _approvalService.GetApprovalByIdAsync(id);
            if (item == null)
            {
                return NotFound(new ErrorResponse { Message = $"Approval request #{id} not found." });
            }

            // Strict visibility rule: Regular users can ONLY view their own approval requests
            if (!isManagerOrAdmin && item.UserId != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse { Message = "Access denied. You can only view your own approval requests." });
            }

            return Ok(new ApiResponse<ApprovalRequestDto>
            {
                Success = true,
                Message = "Approval request details retrieved.",
                Data = item
            });
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ApprovalRequestDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateApproval([FromBody] CreateApprovalRequest request)
        {
            var (userId, _) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            var user = await _userService.GetUserByIdAsync(userId);
            var userName = user?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? "Employee";
            var userEmail = user?.Email ?? User.FindFirstValue(ClaimTypes.Email) ?? "employee@example.com";
            
            // Resolve department name if available
            string? departmentName = null;
            if (user?.DesignationId.HasValue == true)
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                var matchingDept = departments.FirstOrDefault(d => d.Designations.Any(des => des.Id == user.DesignationId.Value));
                departmentName = matchingDept?.Name;
            }

            var created = await _approvalService.CreateApprovalAsync(request, userId, userName, userEmail, departmentName);

            return CreatedAtAction(nameof(GetApprovalById), new { id = created.Id }, new ApiResponse<ApprovalRequestDto>
            {
                Success = true,
                Message = "Approval request submitted successfully for manager review.",
                Data = created
            });
        }

        [HttpPut("{id:int}/action")]
        [ProducesResponseType(typeof(ApiResponse<ApprovalRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ProcessAction(int id, [FromBody] ApprovalActionRequest request)
        {
            var (userId, isManagerOrAdmin) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            // Strict Role Governance: ONLY Manager and Super Admin can approve or reject
            if (!isManagerOrAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access denied. Only managers and super administrators are authorized to approve or reject approval requests."
                });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            var reviewerName = user?.Name ?? User.FindFirstValue(ClaimTypes.Name) ?? "Manager";

            var updated = await _approvalService.ProcessActionAsync(id, request, userId, reviewerName);
            if (updated == null)
            {
                return NotFound(new ErrorResponse { Message = $"Approval request #{id} not found." });
            }

            var actionWord = string.Equals(request.Action, "Approve", StringComparison.OrdinalIgnoreCase) ? "approved" : "rejected";

            return Ok(new ApiResponse<ApprovalRequestDto>
            {
                Success = true,
                Message = $"Request #{id} has been successfully {actionWord}!",
                Data = updated
            });
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteApproval(int id)
        {
            var (userId, isManagerOrAdmin) = await GetCallerContextAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid user token required." });

            var success = await _approvalService.DeleteApprovalAsync(id, userId, isManagerOrAdmin);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Approval request #{id} not found or cannot be cancelled." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Approval request removed successfully.",
                Id = id,
                DeletedFlag = 0
            });
        }

        private async Task<(int UserId, bool IsManagerOrAdmin)> GetCallerContextAsync()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) || userId <= 0)
            {
                return (0, false);
            }

            // Look up actual user record from database
            var dbUser = await _userService.GetUserByIdAsync(userId);
            if (dbUser == null) return (userId, false);

            var roleId = dbUser.RoleId ?? 0;
            var roleName = (dbUser.RoleName ?? "").Trim().ToLowerInvariant();

            // ONLY Super Admin (RoleId 2) and Manager (RoleId 3) or matching role title can view all & manage approvals
            bool isManagerOrAdmin = roleId == 2 || roleId == 3 || roleName.Contains("manager") || roleName.Contains("super admin") || roleName == "admin";

            return (userId, isManagerOrAdmin);
        }
    }
}
