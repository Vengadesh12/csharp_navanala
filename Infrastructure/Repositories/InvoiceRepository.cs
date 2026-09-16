using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Common.DTO;
using MyBackend.Domain.Models;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<InvoiceModel> Items, int TotalCount)> GetInvoicesPagedAsync(
            string? status,
            DateTime? startDate,
            DateTime? endDate,
            string? search,
            int page,
            int pageSize)
        {
            await UpdateOverdueInvoicesAsync();

            var dbQuery = _context.Invoices
                .Include(i => i.Items.Where(item => item.DeletedFlag == 1))
                .AsNoTracking()
                .Where(i => i.DeletedFlag == 1);

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var statusLower = status.Trim().ToLower();
                dbQuery = dbQuery.Where(i => i.Status.ToLower() == statusLower);
            }

            if (startDate.HasValue)
            {
                dbQuery = dbQuery.Where(i => i.InvoiceDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                dbQuery = dbQuery.Where(i => i.InvoiceDate <= endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                dbQuery = dbQuery.Where(i =>
                    i.InvoiceNumber.ToLower().Contains(s) ||
                    i.CustomerName.ToLower().Contains(s) ||
                    (i.CustomerEmail != null && i.CustomerEmail.ToLower().Contains(s)) ||
                    (i.CustomerPhone != null && i.CustomerPhone.ToLower().Contains(s)) ||
                    (i.CustomerGstin != null && i.CustomerGstin.ToLower().Contains(s)) ||
                    i.Items.Any(it => it.ProductName.ToLower().Contains(s)));
            }

            var totalCount = await dbQuery.CountAsync();
            var pageNum = page > 0 ? page : 1;
            var size = pageSize > 0 ? pageSize : 50;

            var items = await dbQuery
                .OrderByDescending(i => i.CreatedAt)
                .Skip((pageNum - 1) * size)
                .Take(size)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<InvoiceModel?> GetInvoiceByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Items.Where(it => it.DeletedFlag == 1))
                .FirstOrDefaultAsync(i => i.Id == id && i.DeletedFlag == 1);
        }

        public async Task<int> UpdateOverdueInvoicesAsync()
        {
            try
            {
                var todayUtc = DateTime.UtcNow.Date;
                var overdueCandidates = await _context.Invoices
                    .Where(i => i.DeletedFlag == 1 &&
                                i.Status == "Pending" &&
                                i.DueDate != null &&
                                i.DueDate.Value < todayUtc)
                    .ToListAsync();

                if (overdueCandidates.Count == 0) return 0;

                foreach (var inv in overdueCandidates)
                {
                    inv.Status = "Overdue";
                    inv.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return overdueCandidates.Count;
            }
            catch
            {
                // In case of any concurrent conflict or DB issue, fail gracefully
                return 0;
            }
        }

        public async Task<(int TotalInvoices, decimal TotalInvoicedAmount, decimal TotalPaidAmount, decimal TotalPendingAmount, decimal TotalOverdueAmount, decimal TotalGstCollected, int PaidCount, int PendingCount, int DraftCount, int OverdueCount)> GetSummaryAsync()
        {
            await UpdateOverdueInvoicesAsync();

            var activeInvoices = await _context.Invoices
                .AsNoTracking()
                .Where(i => i.DeletedFlag == 1)
                .ToListAsync();

            // Cancelled invoices are voided/cancelled, so exclude them from invoiced amount, total active invoices, and GST
            var validInvoices = activeInvoices
                .Where(i => !i.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var totalInvoices = validInvoices.Count;
            var totalInvoicedAmount = validInvoices.Sum(i => i.TotalAmount);
            var totalPaidAmount = validInvoices.Where(i => i.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase)).Sum(i => i.TotalAmount);
            var totalPendingAmount = validInvoices.Where(i => i.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)).Sum(i => i.TotalAmount);
            var totalOverdueAmount = validInvoices.Where(i => i.Status.Equals("Overdue", StringComparison.OrdinalIgnoreCase)).Sum(i => i.TotalAmount);
            var totalGstCollected = validInvoices.Sum(i => i.TaxAmount);

            var paidCount = validInvoices.Count(i => i.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase));
            var pendingCount = validInvoices.Count(i => i.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
            var draftCount = validInvoices.Count(i => i.Status.Equals("Draft", StringComparison.OrdinalIgnoreCase));
            var overdueCount = validInvoices.Count(i => i.Status.Equals("Overdue", StringComparison.OrdinalIgnoreCase));

            return (totalInvoices, totalInvoicedAmount, totalPaidAmount, totalPendingAmount, totalOverdueAmount, totalGstCollected, paidCount, pendingCount, draftCount, overdueCount);
        }

        public async Task<InvoiceModel> AddInvoiceAsync(InvoiceModel invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<InvoiceModel?> UpdateInvoiceWithItemsAsync(
            int id,
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
            string status,
            string? paymentMethod,
            string? notes,
            string? termsAndConditions,
            List<InvoiceItemModel> newItems)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id && i.DeletedFlag == 1);

            if (invoice == null) return null;

            _context.InvoiceItems.RemoveRange(invoice.Items);

            foreach (var item in newItems)
            {
                item.InvoiceId = invoice.Id;
            }

            invoice.Items = newItems;
            if (!string.IsNullOrWhiteSpace(invoiceNumber)) invoice.InvoiceNumber = invoiceNumber.Trim();
            invoice.CustomerName = customerName.Trim();
            invoice.CustomerEmail = customerEmail?.Trim();
            invoice.CustomerPhone = customerPhone?.Trim();
            invoice.CustomerAddress = customerAddress?.Trim();
            if (customerGstin != null) invoice.CustomerGstin = customerGstin.Trim().ToUpper();
            if (!string.IsNullOrWhiteSpace(companyGstin)) invoice.CompanyGstin = companyGstin.Trim().ToUpper();
            if (invoiceDate.HasValue) invoice.InvoiceDate = invoiceDate.Value;
            if (dueDate.HasValue) invoice.DueDate = dueDate.Value;
            invoice.DiscountAmount = Math.Max(0, discountAmount);
            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = status.Trim();
                invoice.Status = normalizedStatus;

                // When user explicitly sets status to Pending, make sure DueDate isn't in the past
                if (normalizedStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                {
                    var todayUtc = DateTime.UtcNow.Date;
                    if (!invoice.DueDate.HasValue || invoice.DueDate.Value.Date < todayUtc)
                    {
                        invoice.DueDate = todayUtc.AddDays(15);
                    }
                }
            }
            if (paymentMethod != null) invoice.PaymentMethod = paymentMethod.Trim();
            if (notes != null) invoice.Notes = notes.Trim();
            if (termsAndConditions != null) invoice.TermsAndConditions = termsAndConditions.Trim();
            invoice.UpdatedAt = DateTime.UtcNow;

            invoice.RecalculateTotals();

            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<bool> SoftDeleteInvoiceAsync(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id && i.DeletedFlag == 1);

            if (invoice == null) return false;

            invoice.DeletedFlag = 0;
            invoice.UpdatedAt = DateTime.UtcNow;
            foreach (var item in invoice.Items)
            {
                item.DeletedFlag = 0;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string?> GetLatestInvoiceNumberForPrefixAsync(string prefix)
        {
            return await _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(i => i.Id)
                .Select(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber)) return false;
            var num = invoiceNumber.Trim().ToLower();
            var query = _context.Invoices.Where(i => i.InvoiceNumber.ToLower() == num && i.DeletedFlag == 1);
            if (excludeId.HasValue)
            {
                query = query.Where(i => i.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<bool> UpdateInvoiceStatusAsync(int id, string status)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id && i.DeletedFlag == 1);
            if (invoice == null) return false;

            var normalizedStatus = status.Trim();
            invoice.Status = normalizedStatus;
            invoice.UpdatedAt = DateTime.UtcNow;

            // When user explicitly sets status to Pending, make sure DueDate is in the future
            // so UpdateOverdueInvoicesAsync does not immediately revert it to Overdue on the next read!
            if (normalizedStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase))
            {
                var todayUtc = DateTime.UtcNow.Date;
                if (!invoice.DueDate.HasValue || invoice.DueDate.Value.Date < todayUtc)
                {
                    invoice.DueDate = todayUtc.AddDays(15);
                }
            }
            else if (normalizedStatus.Equals("Overdue", StringComparison.OrdinalIgnoreCase))
            {
                var todayUtc = DateTime.UtcNow.Date;
                if (!invoice.DueDate.HasValue || invoice.DueDate.Value.Date >= todayUtc)
                {
                    invoice.DueDate = todayUtc.AddDays(-1);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
