using System;

namespace MyBackend.Domain.Entities.Model
{
    public class ScheduleEvent
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
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public int DeletedFlag { get; set; } = 1;
    }
}
