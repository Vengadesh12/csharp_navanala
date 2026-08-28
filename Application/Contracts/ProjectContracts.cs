using MyBackend.Domain.Entities;

namespace MyBackend.Application.Contracts
{
    public class CreateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "RBAC Rollout";
        public string Status { get; set; } = "In Progress";
        public string Priority { get; set; } = "Medium";
        public string LeadName { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; } = 0;
        public string DueDate { get; set; } = string.Empty;
    }

    public class UpdateProjectRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = "RBAC Rollout";
        public string Status { get; set; } = "In Progress";
        public string Priority { get; set; } = "Medium";
        public string LeadName { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; } = 0;
        public string DueDate { get; set; } = string.Empty;
    }

    public class ProjectsOverviewResponse
    {
        public int ActiveRollouts { get; set; }
        public int OnTrackCount { get; set; }
        public int PendingReviewsCount { get; set; }
        public List<Project> Projects { get; set; } = [];
    }

    public class ProjectCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int DeletedFlag { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CreateProjectCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
    }
}
