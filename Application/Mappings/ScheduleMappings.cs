using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.Contracts;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Mappings
{
    public static class ScheduleMappings
    {
        public static ScheduleEventDto ToDto(this ScheduleEvent evt)
        {
            return new ScheduleEventDto
            {
                Id = evt.Id,
                Title = evt.Title,
                Description = evt.Description ?? string.Empty,
                EventType = evt.EventType,
                EventDate = evt.EventDate,
                StartTime = evt.StartTime,
                EndTime = evt.EndTime,
                Location = evt.Location ?? "Virtual / Workspace",
                Organizer = evt.Organizer,
                Status = evt.Status,
                Priority = evt.Priority,
                AttendeesCount = evt.AttendeesCount,
                CreatedAt = evt.CreatedAt,
                DeletedFlag = evt.DeletedFlag
            };
        }

        public static List<ScheduleEventDto> ToDtoList(this IEnumerable<ScheduleEvent> events)
        {
            return events.Select(e => e.ToDto()).ToList();
        }

        public static EventTypeDto ToDto(this EventType type, int eventCount = 0)
        {
            return new EventTypeDto
            {
                Id = type.Id,
                Name = type.Name,
                Description = type.Description ?? string.Empty,
                Color = type.Color ?? "#3b82f6",
                Icon = type.Icon ?? "Event",
                CreatedAt = type.CreatedAt,
                CreatedBy = type.CreatedBy ?? "System Admin",
                EventCount = eventCount
            };
        }

        public static List<EventTypeDto> ToDtoList(
            this IEnumerable<EventType> types,
            IReadOnlyDictionary<string, int>? eventCountDict = null)
        {
            return types.Select(t =>
            {
                int count = 0;
                if (eventCountDict != null && eventCountDict.TryGetValue(t.Name, out var ec))
                {
                    count = ec;
                }
                return t.ToDto(count);
            }).ToList();
        }
    }
}
