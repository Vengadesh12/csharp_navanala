using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface IScheduleService
    {
        Task<SchedulesOverviewResponse> GetSchedulesAsync(string? eventType, string? search);
        Task<ScheduleEventDto> CreateScheduleAsync(CreateScheduleRequest request, string creatorName);
        Task<ScheduleEventDto?> UpdateScheduleAsync(int id, UpdateScheduleRequest request);
        Task<bool> DeleteScheduleAsync(int id);

        Task<List<EventTypeDto>> GetEventTypesAsync();
        Task<EventTypeDto> CreateEventTypeAsync(CreateEventTypeRequest request, string creatorName);
        Task<bool> DeleteEventTypeAsync(int id);
    }
}
