using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            var sql = new StringBuilder("""
                SELECT id, invoice_number, customer_name, customer_email, customer_phone, customer_address, customer_gstin, company_gstin, invoice_date, due_date, subtotal, discount_amount, tax_amount, total_amount, status, payment_method, notes, terms_and_conditions, created_by, created_at, updated_at, deleted_flag
                FROM invoices
                WHERE deleted_flag = 1
            """);

            var countSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM invoices
                WHERE deleted_flag = 1
            """);

            var parameters = new List<object>();
            int paramIndex = 0;

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var clause = $" AND LOWER(status) = LOWER({{{paramIndex++}}})";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(status.Trim());
            }

            if (startDate.HasValue)
            {
                var clause = $" AND invoice_date >= {{{paramIndex++}}}";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(startDate.Value);
            }

            if (endDate.HasValue)
            {
                var clause = $" AND invoice_date <= {{{paramIndex++}}}";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(endDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim().ToLower()}%";
                var clause = $" AND (LOWER(invoice_number) LIKE {{{paramIndex}}} OR LOWER(customer_name) LIKE {{{paramIndex}}} OR (customer_email IS NOT NULL AND LOWER(customer_email) LIKE {{{paramIndex}}}) OR (customer_phone IS NOT NULL AND LOWER(customer_phone) LIKE {{{paramIndex}}}) OR (customer_gstin IS NOT NULL AND LOWER(customer_gstin) LIKE {{{paramIndex}}}))";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(pattern);
            }

            var totalCount = await _context.Database
                .SqlQueryRaw<int>(countSql.ToString(), parameters.ToArray())
                .SingleOrDefaultAsync();

            sql.Append(" ORDER BY created_at DESC");

            var pageNum = page > 0 ? page : 1;
            var size = pageSize > 0 ? pageSize : 50;
            var offset = (pageNum - 1) * size;

            sql.Append($" LIMIT {{{paramIndex++}}} OFFSET {{{paramIndex++}}}");
            parameters.Add(size);
            parameters.Add(offset);

            var items = await _context.Invoices
                .FromSqlRaw(sql.ToString(), parameters.ToArray())
                .Include(i => i.Items.Where(it => it.DeletedFlag == 1))
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<InvoiceModel?> GetInvoiceByIdAsync(int id)
        {
            var sql = new StringBuilder("""
                SELECT id, invoice_number, customer_name, customer_email, customer_phone, customer_address, customer_gstin, company_gstin, invoice_date, due_date, subtotal, discount_amount, tax_amount, total_amount, status, payment_method, notes, terms_and_conditions, created_by, created_at, updated_at, deleted_flag
                FROM invoices
                WHERE id = {0} AND deleted_flag = 1
                LIMIT 1
            """);

            return await _context.Invoices
                .FromSqlRaw(sql.ToString(), id)
                .Include(i => i.Items.Where(it => it.DeletedFlag == 1))
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<int> UpdateOverdueInvoicesAsync()
        {
            try
            {
                var todayUtc = DateTime.UtcNow.Date;
                var sql = new StringBuilder("""
                    SELECT id, invoice_number, customer_name, customer_email, customer_phone, customer_address, customer_gstin, company_gstin, invoice_date, due_date, subtotal, discount_amount, tax_amount, total_amount, status, payment_method, notes, terms_and_conditions, created_by, created_at, updated_at, deleted_flag
                    FROM invoices
                    WHERE deleted_flag = 1 AND status = 'Pending' AND due_date IS NOT NULL AND due_date < {0}
                """);

                var overdueCandidates = await _context.Invoices
                    .FromSqlRaw(sql.ToString(), todayUtc)
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

            var sql = new StringBuilder("""
                SELECT id, invoice_number, customer_name, customer_email, customer_phone, customer_address, customer_gstin, company_gstin, invoice_date, due_date, subtotal, discount_amount, tax_amount, total_amount, status, payment_method, notes, terms_and_conditions, created_by, created_at, updated_at, deleted_flag
                FROM invoices
                WHERE deleted_flag = 1
            """);

            var activeInvoices = await _context.Invoices
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
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
