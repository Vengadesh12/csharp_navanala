using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;

namespace MyBackend.Api.Controllers
{
    [ApiController]
    [Route("api/departments")]
    [Tags("Departments")]
    [Produces("application/json")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet("overview")]
        [ProducesResponseType(typeof(DepartmentOverviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOverview()
        {
            var overview = await _departmentService.GetDepartmentsOverviewAsync();
            return Ok(overview);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<DepartmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department is null)
            {
                return NotFound(new ErrorResponse { Message = $"Department with ID {id} not found." });
            }
            return Ok(department);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ErrorResponse { Message = "Department name is required." });
            }

            try
            {
                var created = await _departmentService.CreateDepartmentAsync(request);
                return CreatedAtAction(
                    nameof(GetDepartment),
                    new { id = created.Id },
                    new ApiResponse<DepartmentDto>
                    {
                        Success = true,
                        Message = "Department created successfully!",
                        Data = created
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new ErrorResponse { Message = "Department name is required." });
            }

            try
            {
                var updated = await _departmentService.UpdateDepartmentAsync(id, request);
                if (updated is null)
                {
                    return NotFound(new ErrorResponse { Message = $"Department with ID {id} not found." });
                }

                return Ok(new ApiResponse<DepartmentDto>
                {
                    Success = true,
                    Message = "Department updated successfully!",
                    Data = updated
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse { Message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var success = await _departmentService.DeleteDepartmentAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Department with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Department deleted successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }

        [HttpPost("{id:int}/map-designations")]
        [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MapDesignations(int id, [FromBody] MapDepartmentDesignationsRequest request)
        {
            var updated = await _departmentService.MapDesignationsToDepartmentAsync(id, request);
            if (updated is null)
            {
                return NotFound(new ErrorResponse { Message = $"Department with ID {id} not found." });
            }

            return Ok(new ApiResponse<DepartmentDto>
            {
                Success = true,
                Message = "Designations mapped to department successfully!",
                Data = updated
            });
        }
    }
}
