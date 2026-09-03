using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Represents a commercial invoice business object issued to clients/customers with financial calculations and business methods.
    /// </summary>
    [Table("invoices")]
    public class Invoice
    {
        /// <summary>
        /// Unique invoice identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique invoice reference code (e.g. INV-2026-0001).
        /// </summary>
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>
        /// Billed client / customer / organization name.
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Customer contact email address.
        /// </summary>
        public string? CustomerEmail { get; set; }

        /// <summary>
        /// Customer contact phone number.
        /// </summary>
        public string? CustomerPhone { get; set; }

        /// <summary>
        /// Customer billing address.
        /// </summary>
        public string? CustomerAddress { get; set; }

        /// <summary>
        /// Customer's GST identification number.
        /// </summary>
        public string? CustomerGstin { get; set; }

        /// <summary>
        /// MyBackend Technologies Company GST identification number.
        /// Can only be modified by Super Admins / users with full management permissions.
        /// </summary>
        public string CompanyGstin { get; set; } = "36AAAAA0000A1Z5";

        /// <summary>
        /// Date of invoice issuance.
        /// </summary>
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Payment due date.
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Net subtotal before taxes and discounts.
        /// </summary>
        public decimal Subtotal { get; set; } = 0.00m;

        /// <summary>
        /// Applicable GST percentage rate (e.g. 18.00%).
        /// </summary>
        public decimal TaxRate { get; set; } = 18.00m;

        /// <summary>
        /// Calculated GST tax amount.
        /// </summary>
        public decimal TaxAmount { get; set; } = 0.00m;

        /// <summary>
        /// Optional discount amount deducted from total.
        /// </summary>
        public decimal DiscountAmount { get; set; } = 0.00m;

        /// <summary>
        /// Final payable grand total amount.
        /// </summary>
        public decimal TotalAmount { get; set; } = 0.00m;

        /// <summary>
        /// Formal Grand Total amount expressed in words (e.g. "Rupees Fifty-Four Thousand Only").
        /// </summary>
        public string TotalAmountInWords { get; set; } = string.Empty;

        /// <summary>
        /// Payment status: Draft, Pending, Paid, Overdue, Cancelled.
        /// </summary>
        public string Status { get; set; } = "Draft";

        /// <summary>
        /// Payment mode (e.g. Bank Transfer, UPI, Credit Card, Cheque, Cash).
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Additional client notes or delivery instructions.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Legal terms, payment terms, or warranty conditions.
        /// </summary>
        public string? TermsAndConditions { get; set; }

        /// <summary>
        /// ID of user who created this invoice.
        /// </summary>
        public int CreatedByUserId { get; set; }

        /// <summary>
        /// Name of user who created this invoice.
        /// </summary>
        public string? CreatedByName { get; set; }

        /// <summary>
        /// Record creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Record last modification timestamp.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete flag (1 = Active, 0 = Deleted).
        /// </summary>
        public int DeletedFlag { get; set; } = 1;

        /// <summary>
        /// Navigation property for associated line item products.
        /// </summary>
        public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

        #region Business Object Domain Methods

        /// <summary>
        /// Business Object Factory Method to create and calculate a new Invoice.
        /// </summary>
        public static Invoice Create(
            string invoiceNumber,
            string customerName,
            string? customerEmail,
            string? customerPhone,
            string? customerAddress,
            string? customerGstin,
            string companyGstin,
            DateTime? invoiceDate,
            DateTime? dueDate,
            decimal discountAmount,
            string? status,
            string? paymentMethod,
            string? notes,
            string? termsAndConditions,
            int createdByUserId,
            string? createdByName,
            IEnumerable<InvoiceItem>? lineItems = null)
        {
            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber.Trim(),
                CustomerName = customerName.Trim(),
                CustomerEmail = customerEmail?.Trim(),
                CustomerPhone = customerPhone?.Trim(),
                CustomerAddress = customerAddress?.Trim(),
                CustomerGstin = customerGstin?.Trim().ToUpperInvariant(),
                CompanyGstin = string.IsNullOrWhiteSpace(companyGstin) ? "36AAAAA0000A1Z5" : companyGstin.Trim().ToUpperInvariant(),
                InvoiceDate = invoiceDate ?? DateTime.UtcNow,
                DueDate = dueDate ?? DateTime.UtcNow.AddDays(15),
                DiscountAmount = Math.Max(0, discountAmount),
                Status = string.IsNullOrWhiteSpace(status) ? "Draft" : status.Trim(),
                PaymentMethod = paymentMethod?.Trim(),
                Notes = notes?.Trim(),
                TermsAndConditions = termsAndConditions?.Trim(),
                CreatedByUserId = createdByUserId,
                CreatedByName = createdByName,
                CreatedAt = DateTime.UtcNow,
                DeletedFlag = 1,
                Items = lineItems != null ? lineItems.ToList() : new List<InvoiceItem>()
            };

            invoice.RecalculateTotals();
            return invoice;
        }

        /// <summary>
        /// Recalculates subtotal, tax amount, average tax rate, total amount and word representation based on active items and discount.
        /// </summary>
        public void RecalculateTotals()
        {
            var activeItems = Items.Where(i => i.DeletedFlag == 1).ToList();

            decimal subtotal = 0m;
            decimal totalTax = 0m;

            foreach (var item in activeItems)
            {
                item.CalculateAmounts();
                subtotal += item.Quantity * item.UnitPrice;
                totalTax += item.TaxAmount;
            }

            Subtotal = subtotal;
            TaxAmount = totalTax;
            TaxRate = activeItems.Count > 0 ? Math.Round(activeItems.Average(i => i.TaxRate), 2) : 18.00m;

            TotalAmount = Math.Max(0, Subtotal + TaxAmount - DiscountAmount);
            TotalAmountInWords = ConvertAmountToWords(TotalAmount);
        }

        /// <summary>
        /// Updates the invoice general information, customer details, and payment options.
        /// </summary>
        public void UpdateDetails(
            string? invoiceNumber,
            string customerName,
            string? customerEmail,
            string? customerPhone,
            string? customerAddress,
            string? customerGstin,
            string? companyGstin,
            DateTime? invoiceDate,
            DateTime? dueDate,
            decimal discountAmount,
            string? status,
            string? paymentMethod,
            string? notes,
            string? termsAndConditions)
        {
            if (!string.IsNullOrWhiteSpace(invoiceNumber)) InvoiceNumber = invoiceNumber.Trim();
            if (!string.IsNullOrWhiteSpace(customerName)) CustomerName = customerName.Trim();
            CustomerEmail = customerEmail?.Trim();
            CustomerPhone = customerPhone?.Trim();
            CustomerAddress = customerAddress?.Trim();
            CustomerGstin = customerGstin?.Trim().ToUpperInvariant();

            if (!string.IsNullOrWhiteSpace(companyGstin))
            {
                CompanyGstin = companyGstin.Trim().ToUpperInvariant();
            }

            if (invoiceDate.HasValue) InvoiceDate = invoiceDate.Value;
            if (dueDate.HasValue) DueDate = dueDate.Value;
            if (!string.IsNullOrWhiteSpace(status)) Status = status.Trim();
            PaymentMethod = paymentMethod?.Trim();
            Notes = notes?.Trim();
            TermsAndConditions = termsAndConditions?.Trim();
            DiscountAmount = Math.Max(0, discountAmount);
            UpdatedAt = DateTime.UtcNow;

            RecalculateTotals();
        }

        /// <summary>
        /// Soft deletes the invoice and all of its associated line items.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
            foreach (var item in Items)
            {
                item.SoftDelete();
            }
        }

        /// <summary>
        /// Converts decimal currency amount into standard Indian currency words (Rupees and Paise).
        /// </summary>
        public static string ConvertAmountToWords(decimal amount)
        {
            if (amount <= 0) return "Rupees Zero Only";

            long rupees = (long)Math.Floor(amount);
            int paise = (int)Math.Round((amount - rupees) * 100);

            string words = "Rupees " + NumberToIndianWords(rupees);

            if (paise > 0)
            {
                words += " and " + NumberToIndianWords(paise) + " Paise";
            }

            words += " Only";
            return words.Trim();
        }

        private static string NumberToIndianWords(long number)
        {
            if (number == 0) return "Zero";

            var unitsMap = new[]
            {
                "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
                "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
            };

            var tensMap = new[]
            {
                "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
            };

            if (number < 20)
                return unitsMap[number];

            if (number < 100)
            {
                var tens = tensMap[number / 10];
                var remainder = number % 10;
                return remainder > 0 ? $"{tens} {unitsMap[remainder]}" : tens;
            }

            if (number < 1000)
            {
                var hundreds = unitsMap[number / 100] + " Hundred";
                var remainder = number % 100;
                return remainder > 0 ? $"{hundreds} {NumberToIndianWords(remainder)}" : hundreds;
            }

            if (number < 100000)
            {
                var thousands = NumberToIndianWords(number / 1000) + " Thousand";
                var remainder = number % 1000;
                return remainder > 0 ? $"{thousands} {NumberToIndianWords(remainder)}" : thousands;
            }

            if (number < 10000000)
            {
                var lakhs = NumberToIndianWords(number / 100000) + " Lakh";
                var remainder = number % 100000;
                return remainder > 0 ? $"{lakhs} {NumberToIndianWords(remainder)}" : lakhs;
            }

            var crores = NumberToIndianWords(number / 10000000) + " Crore";
            var croreRemainder = number % 10000000;
            return croreRemainder > 0 ? $"{crores} {NumberToIndianWords(croreRemainder)}" : crores;
        }

        #endregion
    }
}
