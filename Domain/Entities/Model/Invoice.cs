using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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

        #region Business Object Domain Methods

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

        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
            foreach (var item in Items)
            {
                item.SoftDelete();
            }
        }

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
