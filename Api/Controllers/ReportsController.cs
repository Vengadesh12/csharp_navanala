using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Tags("Reports")]
    [Produces("application/json")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ReportsOverviewResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReports([FromQuery] string? category, [FromQuery] string? search, CancellationToken cancellationToken)
        {
            var response = await _reportService.GetReportsAsync(category, search, cancellationToken);
            return Ok(response);
        }

        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
        {
            var categories = await _reportService.GetCategoriesAsync(cancellationToken);
            return Ok(categories);
        }

        [HttpGet("{id:int}/download")]
        public async Task<IActionResult> DownloadReport(int id, CancellationToken cancellationToken)
        {
            var result = await _reportService.GetReportDownloadAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound(new ErrorResponse { Message = $"Report with ID {id} not found." });
            }

            Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition");
            return File(result.FileBytes, result.ContentType, result.FileName);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(30 * 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponse<ReportDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateReport([FromForm] CreateReportRequest request, CancellationToken cancellationToken)
        {
            var callerName = User.FindFirstValue(ClaimTypes.Name) ?? "System Administrator";
            var report = await _reportService.CreateReportAsync(request, callerName, cancellationToken);

            return CreatedAtAction(nameof(GetReports), new { id = report.Id }, new ApiResponse<ReportDto>
            {
                Success = true,
                Message = "Report generated and registered in database successfully!",
                Data = report
            });
        }

        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(30 * 1024 * 1024)]
        [ProducesResponseType(typeof(ApiResponse<ReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReport(int id, [FromForm] UpdateReportRequest request, CancellationToken cancellationToken)
        {
            var report = await _reportService.UpdateReportAsync(id, request, cancellationToken);
            if (report == null)
            {
                return NotFound(new ErrorResponse { Message = $"Report with ID {id} not found." });
            }

            return Ok(new ApiResponse<ReportDto>
            {
                Success = true,
                Message = "Report updated successfully!",
                Data = report
            });
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReport(int id, CancellationToken cancellationToken)
        {
            var success = await _reportService.DeleteReportAsync(id, cancellationToken);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Report with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Report deleted successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }
    }
}
