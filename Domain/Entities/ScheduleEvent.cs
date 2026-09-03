using System;

namespace MyBackend.Domain.Entities
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

        #region Business Object Domain Methods

        public static ScheduleEvent Create(
            string title,
            string? description,
            string? eventType,
            string? eventDate,
            string? startTime,
            string? endTime,
            string? location,
            string? organizer,
            string? status,
            string? priority,
            int attendeesCount)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Event title is required.", nameof(title));

            var now = DateTime.UtcNow;
            return new ScheduleEvent
            {
                Title = title.Trim(),
                Description = description?.Trim() ?? string.Empty,
                EventType = string.IsNullOrWhiteSpace(eventType) ? "Audit" : eventType.Trim(),
                EventDate = string.IsNullOrWhiteSpace(eventDate) ? now.AddDays(1).ToString("yyyy-MM-dd") : eventDate.Trim(),
                StartTime = string.IsNullOrWhiteSpace(startTime) ? "10:00 AM" : startTime.Trim(),
                EndTime = string.IsNullOrWhiteSpace(endTime) ? "11:00 AM" : endTime.Trim(),
                Location = string.IsNullOrWhiteSpace(location) ? "Virtual / Workspace" : location.Trim(),
                Organizer = organizer?.Trim() ?? string.Empty,
                Status = string.IsNullOrWhiteSpace(status) ? "Scheduled" : status.Trim(),
                Priority = string.IsNullOrWhiteSpace(priority) ? "Normal" : priority.Trim(),
                AttendeesCount = Math.Max(1, attendeesCount),
                CreatedAt = now,
                UpdatedAt = now,
                DeletedFlag = 1
            };
        }

        public void UpdateDetails(
            string title,
            string? description,
            string? eventType,
            string? eventDate,
            string? startTime,
            string? endTime,
            string? location,
            string? organizer,
            string? status,
            string? priority,
            int attendeesCount)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Event title cannot be empty.", nameof(title));

            Title = title.Trim();
            Description = description?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(eventType)) EventType = eventType.Trim();
            if (!string.IsNullOrWhiteSpace(eventDate)) EventDate = eventDate.Trim();
            if (!string.IsNullOrWhiteSpace(startTime)) StartTime = startTime.Trim();
            if (!string.IsNullOrWhiteSpace(endTime)) EndTime = endTime.Trim();
            if (!string.IsNullOrWhiteSpace(location)) Location = location.Trim();
            if (!string.IsNullOrWhiteSpace(organizer)) Organizer = organizer.Trim();
            if (!string.IsNullOrWhiteSpace(status)) Status = status.Trim();
            if (!string.IsNullOrWhiteSpace(priority)) Priority = priority.Trim();
            AttendeesCount = Math.Max(1, attendeesCount);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reschedule(string newEventDate, string newStartTime, string newEndTime, string? newLocation = null)
        {
            if (!string.IsNullOrWhiteSpace(newEventDate)) EventDate = newEventDate.Trim();
            if (!string.IsNullOrWhiteSpace(newStartTime)) StartTime = newStartTime.Trim();
            if (!string.IsNullOrWhiteSpace(newEndTime)) EndTime = newEndTime.Trim();
            if (!string.IsNullOrWhiteSpace(newLocation)) Location = newLocation.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            DeletedFlag = 1;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
