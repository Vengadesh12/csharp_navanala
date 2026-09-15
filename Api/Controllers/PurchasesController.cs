using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using IAppAuthorizationService = MyBackend.Application.Interfaces.IAuthorizationService;

namespace MyBackend.Api.Controllers
{
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
        private readonly IAppAuthorizationService _authorizationService;

        public PurchasesController(
            IPurchaseService purchaseService,
            IUserService userService,
            IDesignationService designationService,
            IAppAuthorizationService authorizationService)
        {
            _purchaseService = purchaseService;
            _userService = userService;
            _designationService = designationService;
            _authorizationService = authorizationService;
        }

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

            if (await _authorizationService.IsSuperAdminAsync(userId))
            {
                return (userId, true, dbUser.Name);
            }

            if (await _authorizationService.HasPermissionAsync(userId, "purchases.view") ||
                await _authorizationService.HasPermissionAsync(userId, "purchases.manage") ||
                await _authorizationService.HasPermissionAsync(userId, "purchases.create"))
            {
                return (userId, true, dbUser.Name);
            }

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

            bool isManager = roleName.Contains("manager") || designationTitle.Contains("manager") || roleName.Contains("lead");
            bool isHrDepartment = departmentName.Contains("hr") || departmentName.Contains("human resources") || designationTitle.Contains("hr");

            bool isAuthorized = isManager || isHrDepartment;

            return (userId, isAuthorized, dbUser.Name);
        }
    }
}
