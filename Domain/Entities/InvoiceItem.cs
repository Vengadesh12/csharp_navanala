using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Represents an individual product line item business object within an invoice.
    /// </summary>
    [Table("invoice_items")]
    public class InvoiceItem
    {
        /// <summary>
        /// Unique line item identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key linking to parent Invoice.
        /// </summary>
        public int InvoiceId { get; set; }

        /// <summary>
        /// Name of the product or service.
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description or product SKU/HSN code.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Quantity of product billed.
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Unit rate / price per quantity.
        /// </summary>
        public decimal UnitPrice { get; set; } = 0.00m;

        /// <summary>
        /// Applicable GST percentage for this line item (e.g. 18.00%).
        /// </summary>
        public decimal TaxRate { get; set; } = 18.00m;

        /// <summary>
        /// Tax amount computed for this line item.
        /// </summary>
        public decimal TaxAmount { get; set; } = 0.00m;

        /// <summary>
        /// Line item total (Quantity * UnitPrice + TaxAmount).
        /// </summary>
        public decimal TotalAmount { get; set; } = 0.00m;

        /// <summary>
        /// Display sequence order.
        /// </summary>
        public int OrderIndex { get; set; } = 0;

        /// <summary>
        /// Soft delete flag (1 = Active, 0 = Deleted).
        /// </summary>
        public int DeletedFlag { get; set; } = 1;

        /// <summary>
        /// Record creation timestamp.
        /// </summary>
        public System.DateTime CreatedAt { get; set; } = System.DateTime.UtcNow;

        /// <summary>
        /// Record last modification timestamp.
        /// </summary>
        public System.DateTime? UpdatedAt { get; set; } = System.DateTime.UtcNow;

        /// <summary>
        /// Parent invoice navigation reference.
        /// </summary>
        public virtual Invoice? Invoice { get; set; }

        #region Business Object Domain Methods

        /// <summary>
        /// Business Object Factory Method to create and compute a new Invoice Item line.
        /// </summary>
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

        /// <summary>
        /// Recalculates tax amount and total line amount.
        /// </summary>
        public void CalculateAmounts()
        {
            var baseAmount = Quantity * UnitPrice;
            TaxAmount = Math.Round((baseAmount * TaxRate) / 100m, 2);
            TotalAmount = baseAmount + TaxAmount;
            UpdatedAt = System.DateTime.UtcNow;
        }

        /// <summary>
        /// Soft deletes the item line.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = System.DateTime.UtcNow;
        }

        #endregion
    }
}
