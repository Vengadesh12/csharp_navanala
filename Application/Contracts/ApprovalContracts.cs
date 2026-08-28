using System;
using System.Collections.Generic;

namespace MyBackend.Application.Contracts
{
    /// <summary>
    /// Data transfer object for approval request details.
    /// </summary>
    public class ApprovalRequestDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public string Priority { get; set; } = "Medium";
        public decimal? EstimatedAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Comments { get; set; }
        public int? ReviewedById { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int DeletedFlag { get; set; } = 1;
    }

    /// <summary>
    /// Payload submitted by an employee to raise a new product/resource approval request.
    /// </summary>
    public class CreateApprovalRequest
    {
        /// <summary>
        /// Name of the product/item requested (e.g. MacBook Pro, 4K Monitor, Standing Desk).
        /// </summary>
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// Category (Hardware &amp; Devices, Software &amp; Tools, Office Equipment, Accessories, Other).
        /// </summary>
        public string Category { get; set; } = "Hardware & Devices";

        /// <summary>
        /// Justification or reason for the request.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Quantity requested.
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Priority: Low, Medium, High, Urgent.
        /// </summary>
        public string Priority { get; set; } = "Medium";

        /// <summary>
        /// Estimated cost (optional).
        /// </summary>
        public decimal? EstimatedAmount { get; set; }
    }

    /// <summary>
    /// Payload submitted by a manager to approve or reject an employee's request.
    /// </summary>
    public class ApprovalActionRequest
    {
        /// <summary>
        /// Action to take: "Approve" or "Reject".
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Review remarks, approval notes, or reason for rejection.
        /// </summary>
        public string? Comments { get; set; }
    }

    /// <summary>
    /// Overall KPI metrics for the approvals workspace.
    /// </summary>
    public class ApprovalSummaryDto
    {
        public int TotalRequests { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int MyRequestsCount { get; set; }
    }

    /// <summary>
    /// Query filters for retrieving approval requests.
    /// </summary>
    public class ApprovalQueryParameters
    {
        public string? Status { get; set; }
        public string? Category { get; set; }
        public string? Priority { get; set; }
        public string? Search { get; set; }
        public string? Scope { get; set; } // "all" or "my"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    /// <summary>
    /// Paginated response list of approval requests.
    /// </summary>
    public class PagedApprovalResponse
    {
        public List<ApprovalRequestDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public ApprovalSummaryDto Summary { get; set; } = new();
    }
}
