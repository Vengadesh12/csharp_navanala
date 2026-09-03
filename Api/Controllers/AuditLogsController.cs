using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    [ApiController]
    [Route("api/audit")]
    [Tags("Audit Logs")]
    [Produces("application/json")]
    [Authorize]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(AuditLogOverviewResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogs([FromQuery] string? module, [FromQuery] string? search)
        {
            var response = await _auditLogService.GetAuditLogsAsync(module, search);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AuditLogDto>), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateAuditLog([FromBody] CreateAuditLogRequest request)
        {
            var callerName = User.FindFirstValue(ClaimTypes.Name) ?? "System Administrator";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            var log = await _auditLogService.CreateAuditLogAsync(request, callerName, ipAddress);

            return CreatedAtAction(nameof(GetAuditLogs), new { id = log.Id }, new ApiResponse<AuditLogDto>
            {
                Success = true,
                Message = "Audit log recorded successfully!",
                Data = log
            });
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAuditLog(int id)
        {
            var success = await _auditLogService.DeleteAuditLogAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Audit log with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Audit log removed successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }
    }
}
