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

        public void RecalculateTotals()
        {
            var activeItems = (Items ?? Enumerable.Empty<InvoiceItem>())
                .Where(i => i.DeletedFlag == 1)
                .ToList();

            decimal subtotal = 0m;
            decimal totalTax = 0m;

            foreach (var item in activeItems)
            {
                var baseAmount = item.Quantity * item.UnitPrice;
                item.TaxAmount = Math.Round((baseAmount * item.TaxRate) / 100m, 2);
                item.TotalAmount = baseAmount + item.TaxAmount;
                item.UpdatedAt = DateTime.UtcNow;

                subtotal += item.Quantity * item.UnitPrice;
                totalTax += item.TaxAmount;
            }

            Subtotal = subtotal;
            TaxAmount = totalTax;
            TaxRate = activeItems.Count > 0 ? Math.Round(activeItems.Average(i => i.TaxRate), 2) : 18.00m;
            TotalAmount = Math.Max(0, Subtotal + TaxAmount - DiscountAmount);
            TotalAmountInWords = ConvertAmountToWords(TotalAmount);
            UpdatedAt = DateTime.UtcNow;
        }

        private static string ConvertAmountToWords(decimal amount)
        {
            try
            {
                long rupees = (long)Math.Truncate(amount);
                int paise = (int)Math.Round((amount - rupees) * 100);

                string words = NumberToIndianWords(rupees);
                if (string.IsNullOrWhiteSpace(words))
                {
                    words = "Zero";
                }

                words = $"{words} Rupees";

                if (paise > 0)
                {
                    words = $"{words} and {NumberToIndianWords(paise)} Paise";
                }

                return $"{words} Only";
            }
            catch
            {
                return $"{amount:C} Only";
            }
        }

        private static string NumberToIndianWords(long number)
        {
            if (number == 0) return "Zero";
            if (number < 0) return "Minus " + NumberToIndianWords(Math.Abs(number));

            string[] unitsMap = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
                "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tensMap = { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            string words = "";

            if ((number / 10000000) > 0)
            {
                words += NumberToIndianWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }

            if ((number / 100000) > 0)
            {
                words += NumberToIndianWords(number / 100000) + " Lakh ";
                number %= 100000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToIndianWords(number / 1000) + " Thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToIndianWords(number / 100) + " Hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += " " + unitsMap[number % 10];
                }
            }

            return words.Trim();
        }
    }
}
