using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    public interface IScheduleRepository
    {
        Task<(List<ScheduleEvent> Schedules, int UpcomingReviews, int DueThisWeek, int TotalUsers, int ActiveSessions)> GetSchedulesOverviewDataAsync(string? eventType, string? search);

        Task<(List<EventType> EventTypes, List<string> ActiveScheduleTypes)> GetEventTypesWithCountsAsync();

        Task<EventType?> GetEventTypeByNameAsync(string name);

        Task<bool> UpdateEventTypeAsync(int id, string description, string color, string icon);

        Task<int> GetActiveEventCountForTypeAsync(string typeName);

        Task<int> CreateEventTypeAsync(string name, string description, string color, string icon, string createdBy);

        Task<bool> SoftDeleteEventTypeAsync(int id);

        Task<int> CreateScheduleAsync(string title, string description, string eventType, string eventDate, string startTime, string endTime, string location, string organizer, string status, string priority, int attendeesCount);

        Task<ScheduleEvent?> GetScheduleByIdAsync(int id);

        Task<bool> UpdateScheduleAsync(int id, string title, string description, string eventType, string eventDate, string startTime, string endTime, string location, string organizer, string status, string priority, int attendeesCount);

        Task<bool> SoftDeleteScheduleAsync(int id);
    }
}
