using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Contracts;
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

        /// <summary>
        /// Retrieve all compliance and security reports.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ReportsOverviewResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReports([FromQuery] string? category, [FromQuery] string? search)
        {
            var response = await _reportService.GetReportsAsync(category, search);
            return Ok(response);
        }

        /// <summary>
        /// Retrieve all unique report categories.
        /// </summary>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _reportService.GetCategoriesAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Download / Export report data.
        /// </summary>
        [HttpGet("{id:int}/download")]
        public async Task<IActionResult> DownloadReport(int id)
        {
            var result = await _reportService.GetReportDownloadAsync(id);
            if (result == null)
            {
                return NotFound(new ErrorResponse { Message = $"Report with ID {id} not found." });
            }

            return File(result.FileBytes, result.ContentType, result.FileName);
        }

        /// <summary>
        /// Generate / Create a new compliance report in the database.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ReportDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
        {
            var callerName = User.FindFirstValue(ClaimTypes.Name) ?? "System Administrator";
            var report = await _reportService.CreateReportAsync(request, callerName);

            return CreatedAtAction(nameof(GetReports), new { id = report.Id }, new ApiResponse<ReportDto>
            {
                Success = true,
                Message = "Report generated and registered in database successfully!",
                Data = report
            });
        }

        /// <summary>
        /// Update an existing report's parameters or status.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateReport(int id, [FromBody] UpdateReportRequest request)
        {
            var report = await _reportService.UpdateReportAsync(id, request);
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

        /// <summary>
        /// Delete / soft-delete a report.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReport(int id)
        {
            var success = await _reportService.DeleteReportAsync(id);
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
