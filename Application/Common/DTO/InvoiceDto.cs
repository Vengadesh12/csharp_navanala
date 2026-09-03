using System;
using System.Collections.Generic;

namespace MyBackend.Application.Common.DTO;

public class InvoiceItemDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int OrderIndex { get; set; }
}

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerGstin { get; set; }
    public string CompanyGstin { get; set; } = "36AAAAA0000A1Z5";
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string TotalAmountInWords { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
}

public class CreateInvoiceItemRequest
{
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; } = 0.00m;
    public decimal TaxRate { get; set; } = 18.00m;
}

public class CreateInvoiceRequest
{
    public string? InvoiceNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerGstin { get; set; }
    public string? CompanyGstin { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal DiscountAmount { get; set; } = 0.00m;
    public string Status { get; set; } = "Draft";
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public List<CreateInvoiceItemRequest> Items { get; set; } = new();
}

public class UpdateInvoiceRequest
{
    public string? InvoiceNumber { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerGstin { get; set; }
    public string? CompanyGstin { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal DiscountAmount { get; set; } = 0.00m;
    public string Status { get; set; } = "Draft";
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public string? TermsAndConditions { get; set; }
    public List<CreateInvoiceItemRequest> Items { get; set; } = new();
}

public class InvoiceQueryParameters
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class PagedInvoiceResponse
{
    public bool Success { get; set; } = true;
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<InvoiceDto> Data { get; set; } = new();
}

public class InvoiceSummaryDto
{
    public int TotalInvoices { get; set; }
    public decimal TotalInvoicedAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalPendingAmount { get; set; }
    public decimal TotalGstCollected { get; set; }
    public int PaidCount { get; set; }
    public int PendingCount { get; set; }
    public int DraftCount { get; set; }
    public int OverdueCount { get; set; }
}
