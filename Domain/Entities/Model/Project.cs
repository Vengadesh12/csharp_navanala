using System;

namespace MyBackend.Domain.Entities.Model
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "RBAC Rollout";
        public string Status { get; set; } = "In Progress";
        public string Priority { get; set; } = "Medium";
        public string LeadName { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; } = 0;
        public string DueDate { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

        public static Project Create(
            string name,
            string? description,
            string? category,
            string? status,
            string? priority,
            string? leadName,
            int progressPercentage,
            string? dueDate)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Project name is required.", nameof(name));

            var now = DateTime.UtcNow;
            return new Project
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                Category = string.IsNullOrWhiteSpace(category) ? "RBAC Rollout" : category.Trim(),
                Status = string.IsNullOrWhiteSpace(status) ? "In Progress" : status.Trim(),
                Priority = string.IsNullOrWhiteSpace(priority) ? "Medium" : priority.Trim(),
                LeadName = leadName?.Trim() ?? string.Empty,
                ProgressPercentage = Math.Clamp(progressPercentage, 0, 100),
                DueDate = dueDate?.Trim() ?? string.Empty,
                CreatedAt = now,
                UpdatedAt = now,
                DeletedFlag = 1
            };
        }

        public void UpdateDetails(
            string name,
            string? description,
            string? category,
            string? status,
            string? priority,
            string? leadName,
            int progressPercentage,
            string? dueDate)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Project name cannot be empty.", nameof(name));

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(category)) Category = category.Trim();
            if (!string.IsNullOrWhiteSpace(status)) Status = status.Trim();
            if (!string.IsNullOrWhiteSpace(priority)) Priority = priority.Trim();
            if (!string.IsNullOrWhiteSpace(leadName)) LeadName = leadName.Trim();
            ProgressPercentage = Math.Clamp(progressPercentage, 0, 100);
            if (!string.IsNullOrWhiteSpace(dueDate)) DueDate = dueDate.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateProgress(int percentage, string? status = null)
        {
            ProgressPercentage = Math.Clamp(percentage, 0, 100);
            if (!string.IsNullOrWhiteSpace(status)) Status = status.Trim();
            else if (ProgressPercentage == 100) Status = "Completed";
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
