using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Represents an employee approval request business object with workflow domain behaviors.
    /// </summary>
    [Table("approval_requests")]
    public class ApprovalRequest
    {
        /// <summary>
        /// Unique request identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID of the user / employee who raised the request.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Full name of the employee who submitted the request.
        /// </summary>
        public string EmployeeName { get; set; } = string.Empty;

        /// <summary>
        /// Email address of the requesting employee.
        /// </summary>
        public string EmployeeEmail { get; set; } = string.Empty;

        /// <summary>
        /// Department of the employee (if assigned).
        /// </summary>
        public string? DepartmentName { get; set; }

        /// <summary>
        /// Name of the product / resource requested (e.g. "MacBook Pro 16-inch M3", "27-inch 4K Monitor").
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// Category (Hardware &amp; Devices, Software &amp; Tools, Office Equipment, Accessories, Other).
        /// </summary>
        public string Category { get; set; } = "Hardware & Devices";

        /// <summary>
        /// Detailed justification / business reason for the approval request.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Quantity requested.
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Priority of request: Low, Medium, High, Urgent.
        /// </summary>
        public string Priority { get; set; } = "Medium";

        /// <summary>
        /// Estimated cost or expense amount (if applicable).
        /// </summary>
        public decimal? EstimatedAmount { get; set; }

        /// <summary>
        /// Status of request: Pending, Approved, Rejected.
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Manager review comments or rejection reason.
        /// </summary>
        public string? Comments { get; set; }

        /// <summary>
        /// User ID of the reviewing manager / admin.
        /// </summary>
        public int? ReviewedById { get; set; }

        /// <summary>
        /// Name of the reviewing manager / admin.
        /// </summary>
        public string? ReviewedByName { get; set; }

        /// <summary>
        /// Timestamp when the request was reviewed (approved / rejected).
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Date when the request was submitted.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date when the request was last modified.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft-delete status flag (1 = Active, 0 = Deleted).
        /// </summary>
        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

        /// <summary>
        /// Business Object Factory Method to create a new pending Approval Request.
        /// </summary>
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

        /// <summary>
        /// Business method: Approves the request.
        /// </summary>
        public void Approve(int reviewerId, string reviewerName, string? comments)
        {
            Status = "Approved";
            Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            ReviewedById = reviewerId;
            ReviewedByName = reviewerName;
            ReviewedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Business method: Rejects the request with manager justification.
        /// </summary>
        public void Reject(int reviewerId, string reviewerName, string? comments)
        {
            Status = "Rejected";
            Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            ReviewedById = reviewerId;
            ReviewedByName = reviewerName;
            ReviewedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Business method: Checks if request is currently pending.
        /// </summary>
        public bool IsPending() => string.Equals(Status, "Pending", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Soft-deletes / cancels the approval request.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
