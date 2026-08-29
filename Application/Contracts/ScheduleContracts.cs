using System;
using System.Collections.Generic;

namespace MyBackend.Application.Contracts
{
    public class ScheduleEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EventType { get; set; } = "Audit";
        public string EventDate { get; set; } = string.Empty;
        public string StartTime { get; set; } = "10:00 AM";
        public string EndTime { get; set; } = "11:00 AM";
        public string Location { get; set; } = "Virtual / Workspace";
        public string Organizer { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public string Priority { get; set; } = "Normal";
        public int AttendeesCount { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int DeletedFlag { get; set; } = 1;
    }

    public class CreateScheduleRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EventType { get; set; } = "Audit";
        public string EventDate { get; set; } = string.Empty;
        public string StartTime { get; set; } = "10:00 AM";
        public string EndTime { get; set; } = "11:00 AM";
        public string Location { get; set; } = "Virtual / Workspace";
        public string Organizer { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public string Priority { get; set; } = "Normal";
        public int AttendeesCount { get; set; } = 1;
    }

    public class UpdateScheduleRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EventType { get; set; } = "Audit";
        public string EventDate { get; set; } = string.Empty;
        public string StartTime { get; set; } = "10:00 AM";
        public string EndTime { get; set; } = "11:00 AM";
        public string Location { get; set; } = "Virtual / Workspace";
        public string Organizer { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public string Priority { get; set; } = "Normal";
        public int AttendeesCount { get; set; } = 1;
    }

    public class SchedulesOverviewResponse
    {
        public int UpcomingReviews { get; set; }
        public string TeamAvailability { get; set; } = "95%";
        public int DueThisWeek { get; set; }
        public List<ScheduleEventDto> Schedules { get; set; } = [];
        public List<EventTypeDto> EventTypes { get; set; } = [];
    }

    public class EventTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#3b82f6";
        public string Icon { get; set; } = "Event";
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = "System Admin";
        public int EventCount { get; set; }
    }

    public class CreateEventTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#3b82f6";
        public string Icon { get; set; } = "Event";
    }

    public class UpdateEventTypeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#3b82f6";
        public string Icon { get; set; } = "Event";
    }
}
