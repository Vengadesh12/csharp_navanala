using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities.Model
{
    [Table("approval_requests")]
    public class ApprovalRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public string EmployeeEmail { get; set; } = string.Empty;

        public string? DepartmentName { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string Category { get; set; } = "Hardware & Devices";

        public string Description { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;

        public string Priority { get; set; } = "Medium";

        public decimal? EstimatedAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public string? Comments { get; set; }

        public int? ReviewedById { get; set; }

        public string? ReviewedByName { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int DeletedFlag { get; set; } = 1;
    }
}
