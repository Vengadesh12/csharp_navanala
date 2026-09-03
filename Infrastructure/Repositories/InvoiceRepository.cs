using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Common.Helpers;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;
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

        public async Task<(List<Invoice> Items, int TotalCount)> GetInvoicesPagedAsync(
            string? status,
            DateTime? startDate,
            DateTime? endDate,
            string? search,
            int page,
            int pageSize)
        {
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

        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Items.Where(it => it.DeletedFlag == 1))
                .FirstOrDefaultAsync(i => i.Id == id && i.DeletedFlag == 1);
        }

        public async Task<(int TotalInvoices, decimal TotalInvoicedAmount, decimal TotalPaidAmount, decimal TotalPendingAmount, decimal TotalGstCollected, int PaidCount, int PendingCount, int DraftCount, int OverdueCount)> GetSummaryAsync()
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

            return (totalInvoices, totalInvoicedAmount, totalPaidAmount, totalPendingAmount, totalGstCollected, paidCount, pendingCount, draftCount, overdueCount);
        }

        public async Task<Invoice> AddInvoiceAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice?> UpdateInvoiceWithItemsAsync(
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
            List<InvoiceItem> newItems)
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
            if (!string.IsNullOrWhiteSpace(status)) invoice.Status = status.Trim();
            if (paymentMethod != null) invoice.PaymentMethod = paymentMethod.Trim();
            if (notes != null) invoice.Notes = notes.Trim();
            if (termsAndConditions != null) invoice.TermsAndConditions = termsAndConditions.Trim();
            invoice.UpdatedAt = DateTime.UtcNow;

            InvoiceCalculationHelper.RecalculateTotals(invoice);

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
    }
}
