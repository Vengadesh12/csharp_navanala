using System;
using System.Collections.Generic;

namespace MyBackend.Application.DTO;

public class PurchaseDto
{
    public int Id { get; set; }
    public int ApprovalRequestId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? EstimatedAmount { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? VendorContact { get; set; }
    public string? VendorEmail { get; set; }
    public string? QuotationNumber { get; set; }
    public decimal QuotationAmount { get; set; }
    public DateTime QuotationDate { get; set; }
    public string? DeliveryTimeline { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Quotation Received";
    public int CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreatePurchaseRequest
{
    public int ApprovalRequestId { get; set; }

    public string VendorName { get; set; } = string.Empty;
    public string? VendorContact { get; set; }
    public string? VendorEmail { get; set; }
    public string? QuotationNumber { get; set; }
    public decimal QuotationAmount { get; set; }
    public DateTime? QuotationDate { get; set; }
    public string? DeliveryTimeline { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public string? Status { get; set; } = "Quotation Received";
}

public class UpdatePurchaseRequest
{
    public string VendorName { get; set; } = string.Empty;
    public string? VendorContact { get; set; }
    public string? VendorEmail { get; set; }
    public string? QuotationNumber { get; set; }
    public decimal QuotationAmount { get; set; }
    public DateTime? QuotationDate { get; set; }
    public string? DeliveryTimeline { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = "Quotation Received";
}

public class ApprovedProductDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal? EstimatedAmount { get; set; }
    public string Priority { get; set; } = "Medium";
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByName { get; set; }
    public bool HasExistingQuotation { get; set; }
    public int QuotationCount { get; set; }
    public int? ExistingPurchaseId { get; set; }
}

public class PurchaseSummaryDto
{
    public int TotalPurchases { get; set; }
    public decimal TotalQuotationValue { get; set; }
    public int QuotationReceivedCount { get; set; }
    public int PoIssuedCount { get; set; }
    public int InProcurementCount { get; set; }
    public int DeliveredCount { get; set; }
    public int CompletedCount { get; set; }
    public int ApprovedItemsPendingQuotation { get; set; }
}

public class PurchaseQueryParameters
{
    public string? Status { get; set; }
    public string? Category { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class PagedPurchaseResponse
{
    public List<PurchaseDto> Data { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
