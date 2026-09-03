using System;
using System.Collections.Generic;

namespace MyBackend.Application.Common.DTO;

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

public class CreateApprovalRequest
{
    public string ItemName { get; set; } = string.Empty;

    public string Category { get; set; } = "Hardware & Devices";

    public string Description { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public string Priority { get; set; } = "Medium";

    public decimal? EstimatedAmount { get; set; }
}

public class ApprovalActionRequest
{
    public string Action { get; set; } = string.Empty;

    public string? Comments { get; set; }
}

public class ApprovalSummaryDto
{
    public int TotalRequests { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int MyRequestsCount { get; set; }
}

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

public class PagedApprovalResponse
{
    public List<ApprovalRequestDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public ApprovalSummaryDto Summary { get; set; } = new();
}
