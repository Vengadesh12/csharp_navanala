using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<(List<Invoice> Items, int TotalCount)> GetInvoicesPagedAsync(
            string? status,
            DateTime? startDate,
            DateTime? endDate,
            string? search,
            int page,
            int pageSize);

        Task<Invoice?> GetInvoiceByIdAsync(int id);

        Task<(int TotalInvoices, decimal TotalInvoicedAmount, decimal TotalPaidAmount, decimal TotalPendingAmount, decimal TotalGstCollected, int PaidCount, int PendingCount, int DraftCount, int OverdueCount)> GetSummaryAsync();

        Task<Invoice> AddInvoiceAsync(Invoice invoice);

        Task<Invoice?> UpdateInvoiceWithItemsAsync(
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
            List<InvoiceItem> newItems);

        Task<bool> SoftDeleteInvoiceAsync(int id);

        Task<string?> GetLatestInvoiceNumberForPrefixAsync(string prefix);
    }
}
