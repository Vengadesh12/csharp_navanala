using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities.Model
{
    [Table("invoices")]
    public class Invoice
    {
        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public string? CustomerEmail { get; set; }

        public string? CustomerPhone { get; set; }

        public string? CustomerAddress { get; set; }

        public string? CustomerGstin { get; set; }

        public string CompanyGstin { get; set; } = "36AAAAA0000A1Z5";

        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        public decimal Subtotal { get; set; } = 0.00m;

        public decimal TaxRate { get; set; } = 18.00m;

        public decimal TaxAmount { get; set; } = 0.00m;

        public decimal DiscountAmount { get; set; } = 0.00m;

        public decimal TotalAmount { get; set; } = 0.00m;

        public string TotalAmountInWords { get; set; } = string.Empty;

        public string Status { get; set; } = "Draft";

        public string? PaymentMethod { get; set; }

        public string? Notes { get; set; }

        public string? TermsAndConditions { get; set; }

        public int CreatedByUserId { get; set; }

        public string? CreatedByName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int DeletedFlag { get; set; } = 1;

        public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }
}
