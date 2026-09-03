using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities
{
    [Table("access_requests")]
    public class AccessRequest
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string? DepartmentName { get; set; }

        public string? RoleName { get; set; }

        public string PermissionKey { get; set; } = string.Empty;

        public string PermissionName { get; set; } = string.Empty;

        public string? Module { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string Priority { get; set; } = "Medium";

        public string Status { get; set; } = "Pending";

        public int? ReviewerId { get; set; }

        public string? ReviewerName { get; set; }

        public string? ReviewerComments { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

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

        public void Approve(int reviewerId, string reviewerName, string? comments)
        {
            Status = "Approved";
            ReviewerId = reviewerId;
            ReviewerName = reviewerName;
            ReviewerComments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            ReviewedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject(int reviewerId, string reviewerName, string? comments)
        {
            Status = "Rejected";
            ReviewerId = reviewerId;
            ReviewerName = reviewerName;
            ReviewerComments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
            ReviewedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
