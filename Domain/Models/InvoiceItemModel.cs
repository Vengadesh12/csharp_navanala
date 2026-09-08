using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Models
{
    [Table("invoice_items")]
    public class InvoiceItemModel
    {
        public int Id { get; set; }

        public int InvoiceId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; } = 0.00m;

        public decimal TaxRate { get; set; } = 18.00m;

        public decimal TaxAmount { get; set; } = 0.00m;

        public decimal TotalAmount { get; set; } = 0.00m;

        public int OrderIndex { get; set; } = 0;

        public int DeletedFlag { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual InvoiceModel? Invoice { get; set; }
    }
}
