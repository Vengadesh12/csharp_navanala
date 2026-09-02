using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Represents an employee permission access request business object with workflow domain behaviors.
    /// </summary>
    [Table("access_requests")]
    public class AccessRequest
    {
        /// <summary>
        /// Unique access request identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID of the user requesting access.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Full name of the requesting user.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Email address of the requesting user.
        /// </summary>
        public string UserEmail { get; set; } = string.Empty;

        /// <summary>
        /// Department name of the requesting user.
        /// </summary>
        public string? DepartmentName { get; set; }

        /// <summary>
        /// Role name of the requesting user.
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// Unique system permission key requested (e.g. "invoices.create", "audit.view").
        /// </summary>
        public string PermissionKey { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable name of the requested permission.
        /// </summary>
        public string PermissionName { get; set; } = string.Empty;

        /// <summary>
        /// Module/Category grouping (e.g. "Invoice", "Purchases", "User Management", "Audit").
        /// </summary>
        public string? Module { get; set; }

        /// <summary>
        /// Business justification / reason why the user needs this permission.
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Priority of request: Low, Medium, High, Urgent.
        /// </summary>
        public string Priority { get; set; } = "Medium";

        /// <summary>
        /// Status of request: Pending, Approved, Rejected.
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// User ID of the reviewing Super Admin / Manager.
        /// </summary>
        public int? ReviewerId { get; set; }

        /// <summary>
        /// Full name of the reviewing Super Admin / Manager.
        /// </summary>
        public string? ReviewerName { get; set; }

        /// <summary>
        /// Reviewer comments or rejection reason.
        /// </summary>
        public string? ReviewerComments { get; set; }

        /// <summary>
        /// Timestamp when the request was reviewed.
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Timestamp when the request was submitted.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when the request record was last modified.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft-delete status flag (1 = Active, 0 = Deleted).
        /// </summary>
        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

        /// <summary>
        /// Factory method to create a new pending Access Request.
        /// </summary>
        public static AccessRequest Create(
            int userId,
            string userName,
            string userEmail,
            string? departmentName,
            string? roleName,
            string permissionKey,
            string permissionName,
            string? module,
            string reason,
            string? priority)
        {
            if (string.IsNullOrWhiteSpace(permissionKey))
                throw new ArgumentException("Permission key is required.", nameof(permissionKey));
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reason / justification is required.", nameof(reason));

            var now = DateTime.UtcNow;
            return new AccessRequest
            {
                UserId = userId,
                UserName = userName.Trim(),
                UserEmail = userEmail.Trim(),
                DepartmentName = string.IsNullOrWhiteSpace(departmentName) ? null : departmentName.Trim(),
                RoleName = string.IsNullOrWhiteSpace(roleName) ? null : roleName.Trim(),
                PermissionKey = permissionKey.Trim(),
                PermissionName = string.IsNullOrWhiteSpace(permissionName) ? permissionKey.Trim() : permissionName.Trim(),
                Module = string.IsNullOrWhiteSpace(module) ? "General" : module.Trim(),
                Reason = reason.Trim(),
                Priority = string.IsNullOrWhiteSpace(priority) ? "Medium" : priority.Trim(),
                Status = "Pending",
                ReviewerId = null,
                ReviewerName = null,
                ReviewerComments = null,
                ReviewedAt = null,
                CreatedAt = now,
                UpdatedAt = now,
                DeletedFlag = 1
            };
        }

        /// <summary>
        /// Approves the access request.
        /// </summary>
        public void Approve(int reviewerId, string reviewerName, string? comments)
        {
            Status = "Approved";
            ReviewerId = reviewerId;
            ReviewerName = reviewerName;
            ReviewerComments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            ReviewedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Rejects the access request with explanation.
        /// </summary>
        public void Reject(int reviewerId, string reviewerName, string? comments)
        {
            Status = "Rejected";
            ReviewerId = reviewerId;
            ReviewerName = reviewerName;
            ReviewerComments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            ReviewedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Soft-deletes the access request.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
