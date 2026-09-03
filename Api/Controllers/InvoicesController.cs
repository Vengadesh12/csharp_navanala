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
    [Route("api/invoices")]
    [Tags("Invoices")]
    [Produces("application/json")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public InvoicesController(IInvoiceService invoiceService, IUserService userService, IAuthService authService)
        {
            _invoiceService = invoiceService;
            _userService = userService;
            _authService = authService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedInvoiceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetInvoices([FromQuery] InvoiceQueryParameters query)
        {
            var (userId, isAuthorized, _, _) = await GetCallerAuthorizationAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid authenticated user session required." });
            if (!isAuthorized)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access Denied. You do not have permission to view invoices."
                });
            }

            var response = await _invoiceService.GetInvoicesAsync(query);
            return Ok(response);
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(InvoiceSummaryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSummary()
        {
            var (userId, isAuthorized, _, _) = await GetCallerAuthorizationAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid authenticated user session required." });
            if (!isAuthorized)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access Denied. You do not have permission to view invoice metrics."
                });
            }

            var summary = await _invoiceService.GetSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            var (userId, isAuthorized, _, _) = await GetCallerAuthorizationAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid authenticated user session required." });
            if (!isAuthorized)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access Denied. You do not have permission to view invoices."
                });
            }

            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
            {
                return NotFound(new ErrorResponse { Message = $"Invoice with ID {id} was not found." });
            }

            return Ok(invoice);
        }

        [HttpPost]
        [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            var (userId, isAuthorized, userName, canManageGst) = await GetCallerAuthorizationAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid authenticated user session required." });
            if (!isAuthorized)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access Denied. You do not have permission to create invoices."
                });
            }

            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                return BadRequest(new ErrorResponse { Message = "Customer / Client name is required." });
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new ErrorResponse { Message = "Invoice must contain at least one product item." });
            }

            try
            {
                var created = await _invoiceService.CreateInvoiceAsync(request, userId, userName, canManageGst);
                return StatusCode(StatusCodes.Status201Created, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] UpdateInvoiceRequest request)
        {
            var (userId, isAuthorized, userName, canManageGst) = await GetCallerAuthorizationAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid authenticated user session required." });
            if (!isAuthorized)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access Denied. You do not have permission to update invoices."
                });
            }

            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                return BadRequest(new ErrorResponse { Message = "Customer / Client name is required." });
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return BadRequest(new ErrorResponse { Message = "Invoice must contain at least one product item." });
            }

            var updated = await _invoiceService.UpdateInvoiceAsync(id, request, userId, userName, canManageGst);
            if (updated == null)
            {
                return NotFound(new ErrorResponse { Message = $"Invoice with ID {id} was not found." });
            }

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var (userId, isAuthorized, _, _) = await GetCallerAuthorizationAsync();
            if (userId <= 0) return Unauthorized(new ErrorResponse { Message = "Valid authenticated user session required." });
            if (!isAuthorized)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse
                {
                    Message = "Access Denied. You do not have permission to delete invoices."
                });
            }

            var success = await _invoiceService.DeleteInvoiceAsync(id, userId);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Invoice with ID {id} was not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Invoice deleted successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }

        private async Task<(int userId, bool isAuthorized, string userName, bool canManageGst)> GetCallerAuthorizationAsync()
        {
            var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst("sub")?.Value
                        ?? User.FindFirst("id")?.Value;

            if (!int.TryParse(subClaim, out var userId) || userId <= 0)
            {
                return (0, false, "Unknown", false);
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null || user.DeletedFlag == 0)
            {
                return (0, false, "Unknown", false);
            }

            var roleId = user.RoleId;
            var isSuperAdmin = roleId == 2;
            var isManager = roleId == 3;

            // Check permissions
            var permResp = await _authService.GetUserPermissionsAsync(userId);
            var userPerms = permResp?.Permissions ?? new List<string>();
            var hasInvoiceView = isSuperAdmin || isManager || userPerms.Contains("invoices.view") || userPerms.Contains("invoices.manage");
            var canManageGst = isSuperAdmin || userPerms.Contains("invoices.manage") || userPerms.Contains("permissions.manage");

            return (userId, hasInvoiceView, user.Name, canManageGst);
        }
    }
}
