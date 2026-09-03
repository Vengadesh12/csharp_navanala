using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IApplicationDbContext _context;
        private const string DEFAULT_COMPANY_GSTIN = "36AAAAA0000A1Z5";

        public InvoiceService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedInvoiceResponse> GetInvoicesAsync(InvoiceQueryParameters query)
        {
            var dbQuery = _context.Invoices
                .Include(i => i.Items.Where(item => item.DeletedFlag == 1))
                .AsNoTracking()
                .Where(i => i.DeletedFlag == 1);

            if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var statusLower = query.Status.Trim().ToLower();
                dbQuery = dbQuery.Where(i => i.Status.ToLower() == statusLower);
            }

            if (query.StartDate.HasValue)
            {
                dbQuery = dbQuery.Where(i => i.InvoiceDate >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                dbQuery = dbQuery.Where(i => i.InvoiceDate <= query.EndDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(i =>
                    i.InvoiceNumber.ToLower().Contains(search) ||
                    i.CustomerName.ToLower().Contains(search) ||
                    (i.CustomerEmail != null && i.CustomerEmail.ToLower().Contains(search)) ||
                    (i.CustomerPhone != null && i.CustomerPhone.ToLower().Contains(search)) ||
                    (i.CustomerGstin != null && i.CustomerGstin.ToLower().Contains(search)) ||
                    i.Items.Any(it => it.ProductName.ToLower().Contains(search)));
            }

            var totalCount = await dbQuery.CountAsync();
            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 50;

            var rawInvoices = await dbQuery
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = rawInvoices.Select(i => i.ToDto()).ToList();

            return new PagedInvoiceResponse
            {
                Success = true,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Data = items
            };
        }

        public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items.Where(it => it.DeletedFlag == 1))
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id && i.DeletedFlag == 1);

            return invoice?.ToDto();
        }

        public async Task<InvoiceSummaryDto> GetSummaryAsync()
        {
            var activeInvoices = await _context.Invoices
                .AsNoTracking()
                .Where(i => i.DeletedFlag == 1)
                .ToListAsync();

            var totalInvoices = activeInvoices.Count;
            var totalInvoicedAmount = activeInvoices.Sum(i => i.TotalAmount);
            var totalPaidAmount = activeInvoices.Where(i => i.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)).Sum(i => i.TotalAmount);
            var totalPendingAmount = activeInvoices.Where(i => i.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)).Sum(i => i.TotalAmount);
            var totalGstCollected = activeInvoices.Sum(i => i.TaxAmount);

            var paidCount = activeInvoices.Count(i => i.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase));
            var pendingCount = activeInvoices.Count(i => i.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
            var draftCount = activeInvoices.Count(i => i.Status.Equals("Draft", StringComparison.OrdinalIgnoreCase));
            var overdueCount = activeInvoices.Count(i => i.Status.Equals("Overdue", StringComparison.OrdinalIgnoreCase));

            return new InvoiceSummaryDto
            {
                TotalInvoices = totalInvoices,
                TotalInvoicedAmount = totalInvoicedAmount,
                TotalPaidAmount = totalPaidAmount,
                TotalPendingAmount = totalPendingAmount,
                TotalGstCollected = totalGstCollected,
                PaidCount = paidCount,
                PendingCount = pendingCount,
                DraftCount = draftCount,
                OverdueCount = overdueCount
            };
        }

        public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, int userId, string userName, bool canEditGst)
        {
            // Auto generate invoice number if not provided
            var invoiceNumber = string.IsNullOrWhiteSpace(request.InvoiceNumber)
                ? await GenerateInvoiceNumberAsync()
                : request.InvoiceNumber.Trim();

            // GST Number policy: only users with all permissions can set a custom company GSTIN
            var companyGstin = (canEditGst && !string.IsNullOrWhiteSpace(request.CompanyGstin))
                ? request.CompanyGstin.Trim().ToUpper()
                : DEFAULT_COMPANY_GSTIN;

            // Build line items using Business Object factory
            var lineItems = new List<InvoiceItem>();
            int orderIdx = 1;
            foreach (var itemReq in request.Items ?? Enumerable.Empty<CreateInvoiceItemRequest>())
            {
                lineItems.Add(InvoiceItem.Create(
                    productName: itemReq.ProductName,
                    description: itemReq.Description,
                    quantity: itemReq.Quantity,
                    unitPrice: itemReq.UnitPrice,
                    taxRate: itemReq.TaxRate,
                    orderIndex: orderIdx++
                ));
            }

            // Create Invoice using Business Object factory (which computes subtotals, GST and grand totals)
            var invoice = Invoice.Create(
                invoiceNumber: invoiceNumber,
                customerName: request.CustomerName,
                customerEmail: request.CustomerEmail,
                customerPhone: request.CustomerPhone,
                customerAddress: request.CustomerAddress,
                customerGstin: request.CustomerGstin,
                companyGstin: companyGstin,
                invoiceDate: request.InvoiceDate,
                dueDate: request.DueDate,
                discountAmount: request.DiscountAmount,
                status: request.Status,
                paymentMethod: request.PaymentMethod,
                notes: request.Notes,
                termsAndConditions: request.TermsAndConditions,
                createdByUserId: userId,
                createdByName: userName,
                lineItems: lineItems
            );

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice.ToDto();
        }

        public async Task<InvoiceDto?> UpdateInvoiceAsync(int id, UpdateInvoiceRequest request, int userId, string userName, bool canEditGst)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id && i.DeletedFlag == 1);

            if (invoice == null) return null;

            // Clear old items and recreate new items using Business Object factory
            _context.InvoiceItems.RemoveRange(invoice.Items);

            var lineItems = new List<InvoiceItem>();
            int orderIdx = 1;
            foreach (var itemReq in request.Items ?? Enumerable.Empty<CreateInvoiceItemRequest>())
            {
                lineItems.Add(InvoiceItem.Create(
                    productName: itemReq.ProductName,
                    description: itemReq.Description,
                    quantity: itemReq.Quantity,
                    unitPrice: itemReq.UnitPrice,
                    taxRate: itemReq.TaxRate,
                    orderIndex: orderIdx++,
                    invoiceId: invoice.Id
                ));
            }

            var companyGstinToUse = (canEditGst && !string.IsNullOrWhiteSpace(request.CompanyGstin))
                ? request.CompanyGstin.Trim().ToUpper()
                : null;

            invoice.Items = lineItems;
            invoice.UpdateDetails(
                invoiceNumber: request.InvoiceNumber,
                customerName: request.CustomerName,
                customerEmail: request.CustomerEmail,
                customerPhone: request.CustomerPhone,
                customerAddress: request.CustomerAddress,
                customerGstin: request.CustomerGstin,
                companyGstin: companyGstinToUse,
                invoiceDate: request.InvoiceDate,
                dueDate: request.DueDate,
                discountAmount: request.DiscountAmount,
                status: request.Status,
                paymentMethod: request.PaymentMethod,
                notes: request.Notes,
                termsAndConditions: request.TermsAndConditions
            );

            await _context.SaveChangesAsync();
            return invoice.ToDto();
        }

        public async Task<bool> DeleteInvoiceAsync(int id, int userId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id && i.DeletedFlag == 1);

            if (invoice == null) return false;

            invoice.SoftDelete();

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"INV-{year}-";
            var latest = await _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(i => i.Id)
                .Select(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (!string.IsNullOrEmpty(latest) && latest.Length > prefix.Length)
            {
                var seqPart = latest.Substring(prefix.Length);
                if (int.TryParse(seqPart, out int currentSeq))
                {
                    nextSeq = currentSeq + 1;
                }
            }

            return $"{prefix}{nextSeq:D4}";
        }

        /// <summary>
        /// Converts decimal amounts into standard Indian currency words (Rupees and Paise, Lakhs and Crores).
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

            if (number < 100000) // Thousands
            {
                var thousands = NumberToIndianWords(number / 1000) + " Thousand";
                var remainder = number % 1000;
                return remainder > 0 ? $"{thousands} {NumberToIndianWords(remainder)}" : thousands;
            }

            if (number < 10000000) // Lakhs
            {
                var lakhs = NumberToIndianWords(number / 100000) + " Lakh";
                var remainder = number % 100000;
                return remainder > 0 ? $"{lakhs} {NumberToIndianWords(remainder)}" : lakhs;
            }

            // Crores
            var crores = NumberToIndianWords(number / 10000000) + " Crore";
            var croreRemainder = number % 10000000;
            return croreRemainder > 0 ? $"{crores} {NumberToIndianWords(croreRemainder)}" : crores;
        }
    }
}
