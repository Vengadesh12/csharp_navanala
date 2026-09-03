using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities.Model
{
    [Table("purchases")]
    public class Purchase
    {
        public int Id { get; set; }

        public int ApprovalRequestId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string Category { get; set; } = "Hardware & Devices";

        public int Quantity { get; set; } = 1;

        public decimal? EstimatedAmount { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public string EmployeeEmail { get; set; } = string.Empty;

        public string? DepartmentName { get; set; }

        public string VendorName { get; set; } = string.Empty;

        public string? VendorContact { get; set; }

        public string? VendorEmail { get; set; }

        public string? QuotationNumber { get; set; }

        public decimal QuotationAmount { get; set; } = 0.00m;

        public DateTime QuotationDate { get; set; } = DateTime.UtcNow;

        public string? DeliveryTimeline { get; set; }

        public string? PaymentTerms { get; set; }

        public string? Notes { get; set; }

        public string Status { get; set; } = "Quotation Received";

        public int CreatedByUserId { get; set; }

        public string? CreatedByName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int DeletedFlag { get; set; } = 1;
    }
}
