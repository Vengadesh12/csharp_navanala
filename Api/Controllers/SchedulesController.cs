using System.Collections.Generic;
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
    [Route("api/schedules")]
    [Tags("Schedules")]
    [Produces("application/json")]
    [Authorize]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public SchedulesController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(SchedulesOverviewResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSchedules([FromQuery] string? eventType, [FromQuery] string? search)
        {
            var response = await _scheduleService.GetSchedulesAsync(eventType, search);
            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<ScheduleEventDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSchedule([FromBody] CreateScheduleRequest request)
        {
            var callerName = User.FindFirstValue(ClaimTypes.Name) ?? "System Lead";
            var schedule = await _scheduleService.CreateScheduleAsync(request, callerName);

            return CreatedAtAction(nameof(GetSchedules), new { id = schedule.Id }, new ApiResponse<ScheduleEventDto>
            {
                Success = true,
                Message = "Event scheduled and saved in database successfully!",
                Data = schedule
            });
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<ScheduleEventDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateScheduleRequest request)
        {
            var schedule = await _scheduleService.UpdateScheduleAsync(id, request);
            if (schedule == null)
            {
                return NotFound(new ErrorResponse { Message = $"Scheduled event with ID {id} not found." });
            }

            return Ok(new ApiResponse<ScheduleEventDto>
            {
                Success = true,
                Message = "Scheduled event updated successfully!",
                Data = schedule
            });
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var success = await _scheduleService.DeleteScheduleAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Scheduled event with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Event cancelled and removed successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }

        [HttpGet("types")]
        [ProducesResponseType(typeof(List<EventTypeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEventTypes()
        {
            var types = await _scheduleService.GetEventTypesAsync();
            return Ok(types);
        }

        [HttpPost("types")]
        [ProducesResponseType(typeof(ApiResponse<EventTypeDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateEventType([FromBody] CreateEventTypeRequest request)
        {
            var callerName = User.FindFirstValue(ClaimTypes.Name) ?? "System Lead";
            var eventType = await _scheduleService.CreateEventTypeAsync(request, callerName);

            return CreatedAtAction(nameof(GetEventTypes), new { id = eventType.Id }, new ApiResponse<EventTypeDto>
            {
                Success = true,
                Message = $"Event type '{eventType.Name}' created successfully!",
                Data = eventType
            });
        }

        [HttpDelete("types/{id:int}")]
        [ProducesResponseType(typeof(DeleteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEventType(int id)
        {
            var success = await _scheduleService.DeleteEventTypeAsync(id);
            if (!success)
            {
                return NotFound(new ErrorResponse { Message = $"Event type with ID {id} not found." });
            }

            return Ok(new DeleteResponse
            {
                Success = true,
                Message = "Event type removed successfully!",
                Id = id,
                DeletedFlag = 0
            });
        }
    }
}
