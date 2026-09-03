using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities.Model
{
    [Table("invoice_items")]
    public class InvoiceItem
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

        public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;

        public System.DateTime? UpdatedAt { get; set; } = System.DateTime.UtcNow;

        public virtual Invoice? Invoice { get; set; }

        #region Business Object Domain Methods

        public static InvoiceItem Create(
            string productName,
            string? description,
            int quantity,
            decimal unitPrice,
            decimal taxRate,
            int orderIndex,
            int invoiceId = 0)
        {
            var qty = Math.Max(1, quantity);
            var price = Math.Max(0, unitPrice);
            var rate = Math.Max(0, taxRate);
            var baseAmount = qty * price;
            var taxAmount = Math.Round((baseAmount * rate) / 100m, 2);
            var total = baseAmount + taxAmount;
            var now = System.DateTime.UtcNow;

            return new InvoiceItem
            {
                InvoiceId = invoiceId,
                ProductName = productName.Trim(),
                Description = description?.Trim(),
                Quantity = qty,
                UnitPrice = price,
                TaxRate = rate,
                TaxAmount = taxAmount,
                TotalAmount = total,
                OrderIndex = orderIndex,
                DeletedFlag = 1,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        public void CalculateAmounts()
        {
            var baseAmount = Quantity * UnitPrice;
            TaxAmount = Math.Round((baseAmount * TaxRate) / 100m, 2);
            TotalAmount = baseAmount + TaxAmount;
            UpdatedAt = System.DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = System.DateTime.UtcNow;
        }

        #endregion
    }
}
