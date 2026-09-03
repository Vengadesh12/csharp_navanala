using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities
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

        #region Business Object Domain Methods

        public static ApprovalRequest Create(
            int userId,
            string userName,
            string userEmail,
            string? departmentName,
            string itemName,
            string? category,
            string description,
            int quantity,
            string? priority,
            decimal? estimatedAmount)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                throw new ArgumentException("Product / Item name is required.", nameof(itemName));
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Reason / justification is required.", nameof(description));

            var now = DateTime.UtcNow;
            return new ApprovalRequest
            {
                UserId = userId,
                EmployeeName = userName.Trim(),
                EmployeeEmail = userEmail.Trim(),
                DepartmentName = string.IsNullOrWhiteSpace(departmentName) ? null : departmentName.Trim(),
                ItemName = itemName.Trim(),
                Category = string.IsNullOrWhiteSpace(category) ? "Hardware & Devices" : category.Trim(),
                Description = description.Trim(),
                Quantity = quantity > 0 ? quantity : 1,
                Priority = string.IsNullOrWhiteSpace(priority) ? "Medium" : priority.Trim(),
                EstimatedAmount = estimatedAmount,
                Status = "Pending",
                Comments = null,
                ReviewedById = null,
                ReviewedByName = null,
                ReviewedAt = null,
                CreatedAt = now,
                UpdatedAt = now,
                DeletedFlag = 1
            };
        }

        public void Approve(int reviewerId, string reviewerName, string? comments)
        {
            Status = "Approved";
            Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            ReviewedById = reviewerId;
            ReviewedByName = reviewerName;
            ReviewedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject(int reviewerId, string reviewerName, string? comments)
        {
            Status = "Rejected";
            Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            ReviewedById = reviewerId;
            ReviewedByName = reviewerName;
            ReviewedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsPending() => string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase);

        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
