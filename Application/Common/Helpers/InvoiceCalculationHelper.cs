using System;
using System.Linq;
using MyBackend.Domain.Entities.Model;

namespace MyBackend.Application.Common.Helpers
{
    public static class InvoiceCalculationHelper
    {
        public static void CalculateItemAmounts(InvoiceItem item)
        {
            var baseAmount = item.Quantity * item.UnitPrice;
            item.TaxAmount = Math.Round((baseAmount * item.TaxRate) / 100m, 2);
            item.TotalAmount = baseAmount + item.TaxAmount;
            item.UpdatedAt = DateTime.UtcNow;
        }

        public static void RecalculateTotals(Invoice invoice)
        {
            var activeItems = (invoice.Items ?? Enumerable.Empty<InvoiceItem>())
                .Where(i => i.DeletedFlag == 1)
                .ToList();

            decimal subtotal = 0m;
            decimal totalTax = 0m;

            foreach (var item in activeItems)
            {
                CalculateItemAmounts(item);
                subtotal += item.Quantity * item.UnitPrice;
                totalTax += item.TaxAmount;
            }

            invoice.Subtotal = subtotal;
            invoice.TaxAmount = totalTax;
            invoice.TaxRate = activeItems.Count > 0 ? Math.Round(activeItems.Average(i => i.TaxRate), 2) : 18.00m;
            invoice.TotalAmount = Math.Max(0, invoice.Subtotal + invoice.TaxAmount - invoice.DiscountAmount);
            invoice.TotalAmountInWords = ConvertAmountToWords(invoice.TotalAmount);
            invoice.UpdatedAt = DateTime.UtcNow;
        }

        public static string ConvertAmountToWords(decimal amount)
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

        public static string NumberToIndianWords(long number)
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
