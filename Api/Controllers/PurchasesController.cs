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
    /// <summary>
    /// Vendor procurement and quotation management for approved products.
    /// Access restricted strictly to Managers, HR Department members, and Super Admins.
    /// </summary>
    [ApiController]
    [Route("api/purchases")]
    [Tags("Purchases")]
    [Produces("application/json")]
    [Authorize]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;
        private readonly IUserService _userService;
        private readonly IDesignationService _designationService;

        public PurchasesController(
            IPurchaseService purchaseService,
            IUserService userService,
            IDesignationService designationService)
        {
            _purchaseService = purchaseService;
            _userService = userService;
            _designationService = designationService;
        }

        /// <summary>
        /// Retrieves all purchases / vendor quotations with optional search, status, and category filters.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedPurchaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPurchases([FromQuery] PurchaseQueryParameters query)
        {
            var (userId, isAuthorized, userName) = await GetCallerAuthorizationAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid authenticated user session required." });
            if (!isAuthorized)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access Denied. Only Managers, HR Department members, and Super Admins can access Purchases & Procurement."
                });
            }

            var response = await _purchaseService.GetPurchasesAsync(query);
            return Ok(response);
        }

        /// <summary>
        /// Retrieves all approved products available for vendor quotation.
        /// </summary>
        [HttpGet("approved-products")]
        [ProducesResponseType(typeof(ApiResponse<ApprovedProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetApprovedProducts()
        {
            var (userId, isAuthorized, _) = await GetCallerAuthorizationAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid authenticated user session required." });
            if (!isAuthorized)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access Denied. Only Managers, HR Department members, and Super Admins can access Purchases & Procurement."
                });
            }

            var items = await _purchaseService.GetApprovedProductsAsync();
            return Ok(items);
        }

        /// <summary>
        /// Retrieves executive procurement KPI summary metrics.
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(PurchaseSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSummary()
        {
            var (userId, isAuthorized, _) = await GetCallerAuthorizationAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid authenticated user session required." });
            if (!isAuthorized)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access Denied. Only Managers, HR Department members, and Super Admins can access Purchases & Procurement."
                });
            }

            var summary = await _purchaseService.GetSummaryAsync();
            return Ok(summary);
        }

        /// <summary>
        /// Retrieves single purchase details by ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<PurchaseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPurchaseById(int id)
        {
            var (userId, isAuthorized, _) = await GetCallerAuthorizationAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid authenticated user session required." });
            if (!isAuthorized)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access Denied. Only Managers, HR Department members, and Super Admins can access Purchases & Procurement."
                });
            }

            var purchase = await _purchaseService.GetPurchaseByIdAsync(id);
            if (purchase == null)
            {
                return NotFound(new ErrorResponse { Message = $"Purchase order #{id} not found." });
            }

            return Ok(new ApiResponse<PurchaseDto>
            {
                Success = true,
                Message = "Purchase quotation details retrieved.",
                Data = purchase
            });
        }

        /// <summary>
        /// Creates a new vendor quotation for an approved product.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PurchaseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseRequest request)
        {
            var (userId, isAuthorized, userName) = await GetCallerAuthorizationAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid authenticated user session required." });
            if (!isAuthorized)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access Denied. Only Managers, HR Department members, and Super Admins can create Purchase Quotations."
                });
            }

            if (request == null || request.ApprovalRequestId <= 0 || string.IsNullOrWhiteSpace(request.VendorName))
            {
                return BadRequest(new ErrorResponse { Message = "Valid approved request ID and vendor name are required." });
            }

            try
            {
                var created = await _purchaseService.CreatePurchaseAsync(request, userId, userName);
                return StatusCode(StatusCodes.Status201Created, new ApiResponse<PurchaseDto>
                {
                    Success = true,
                    Message = "Vendor quotation recorded successfully.",
                    Data = created
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        /// <summary>
        /// Updates vendor details, quotation amount, or delivery status.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<PurchaseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePurchase(int id, [FromBody] UpdatePurchaseRequest request)
        {
            var (userId, isAuthorized, _) = await GetCallerAuthorizationAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid authenticated user session required." });
            if (!isAuthorized)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access Denied. Only Managers, HR Department members, and Super Admins can update Purchase Quotations."
                });
            }

            if (request == null || string.IsNullOrWhiteSpace(request.VendorName))
            {
                return BadRequest(new ErrorResponse { Message = "Vendor name is required." });
            }

            var updated = await _purchaseService.UpdatePurchaseAsync(id, request);
            if (updated == null)
            {
                return NotFound(new ErrorResponse { Message = $"Purchase order #{id} not found." });
            }

            return Ok(new ApiResponse<PurchaseDto>
            {
                Success = true,
                Message = "Purchase quotation updated successfully.",
                Data = updated
            });
        }

        /// <summary>
        /// Soft-deletes a purchase record.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePurchase(int id)
        {
            var (userId, isAuthorized, _) = await GetCallerAuthorizationAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid authenticated user session required." });
            if (!isAuthorized)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access Denied. Only Managers, HR Department members, and Super Admins can delete Purchase records."
                });
            }

            var success = await _purchaseService.DeletePurchaseAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Purchase record #{id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Purchase record removed successfully.",
                Id = id,
                DeletedFlag = 0
            });
        }

        private async Task<(int UserId, bool IsAuthorized, string UserName)> GetCallerAuthorizationAsync()
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) || userId <= 0)
            {
                return (0, false, string.Empty);
            }

            var dbUser = await _userService.GetUserByIdAsync(userId);
            if (dbUser == null) return (userId, false, string.Empty);

            var roleId = dbUser.RoleId ?? 0;
            var roleName = (dbUser.RoleName ?? "").Trim().ToLowerInvariant();
            var designationTitle = (dbUser.DesignationName ?? "").Trim().ToLowerInvariant();
            string departmentName = string.Empty;

            if (dbUser.DesignationId.HasValue && dbUser.DesignationId.Value > 0)
            {
                var designation = await _designationService.GetDesignationByIdAsync(dbUser.DesignationId.Value);
                if (designation != null && !string.IsNullOrWhiteSpace(designation.DepartmentName))
                {
                    departmentName = designation.DepartmentName.Trim().ToLowerInvariant();
                }
            }

            // Authorization criteria:
            // 1. Super Admin (RoleId 2 or role title contains 'super admin' or 'admin')
            // 2. Manager (RoleId 3 or role/designation title contains 'manager' or 'lead')
            // 3. HR Department (Department title contains 'hr' or 'human resources' or designation contains 'hr')
            bool isSuperAdmin = roleId == 2 || roleName.Contains("super admin") || roleName == "admin";
            bool isManager = roleId == 3 || roleName.Contains("manager") || designationTitle.Contains("manager") || roleName.Contains("lead");
            bool isHrDepartment = departmentName.Contains("hr") || departmentName.Contains("human resources") || designationTitle.Contains("hr");

            bool isAuthorized = isSuperAdmin || isManager || isHrDepartment;

            return (userId, isAuthorized, dbUser.Name);
        }
    }
}
