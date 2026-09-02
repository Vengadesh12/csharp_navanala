using System;
using System.Collections.Generic;

namespace MyBackend.Application.Contracts
{
    /// <summary>
    /// Data transfer object for permission access request details.
    /// </summary>
    public class AccessRequestDto
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
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int DeletedFlag { get; set; } = 1;
    }

    /// <summary>
    /// Payload submitted by an employee to request access for a permission.
    /// </summary>
    public class CreateAccessRequestDto
    {
        public string PermissionKey { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
    }

    /// <summary>
    /// Payload submitted by Super Admin / Manager to approve or reject a request.
    /// </summary>
    public class ReviewAccessRequestDto
    {
        public string? Comments { get; set; }
    }

    /// <summary>
    /// Represents a system permission with status indicator if granted to current user.
    /// </summary>
    public class AvailablePermissionDto
    {
        public int Id { get; set; }
        public string PermissionKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Module { get; set; } = "General";
        public bool IsGranted { get; set; }
        public bool HasPendingRequest { get; set; }
    }

    /// <summary>
    /// Overall summary KPI counts for permission access requests.
    /// </summary>
    public class AccessRequestSummaryDto
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int MyPendingRequests { get; set; }
    }

    /// <summary>
    /// Query filters for retrieving access requests.
    /// </summary>
    public class AccessRequestQueryParameters
    {
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? Search { get; set; }
        public string? Module { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool OnlyMyRequests { get; set; } = false;
    }

    /// <summary>
    /// Paginated response container for access requests.
    /// </summary>
    public class PagedAccessRequestResponse
    {
        public List<AccessRequestDto> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }
}
