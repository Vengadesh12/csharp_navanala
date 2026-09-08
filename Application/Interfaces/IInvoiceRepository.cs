using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<(List<InvoiceModel> Items, int TotalCount)> GetInvoicesPagedAsync(
            string? status,
            DateTime? startDate,
            DateTime? endDate,
            string? search,
            int page,
            int pageSize);

        Task<InvoiceModel?> GetInvoiceByIdAsync(int id);

        Task<(int TotalInvoices, decimal TotalInvoicedAmount, decimal TotalPaidAmount, decimal TotalPendingAmount, decimal TotalOverdueAmount, decimal TotalGstCollected, int PaidCount, int PendingCount, int DraftCount, int OverdueCount)> GetSummaryAsync();

        Task<int> UpdateOverdueInvoicesAsync();
        Task<bool> UpdateInvoiceStatusAsync(int id, string status);

        Task<InvoiceModel> AddInvoiceAsync(InvoiceModel invoice);

        Task<InvoiceModel?> UpdateInvoiceWithItemsAsync(
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
            List<InvoiceItemModel> newItems);

        Task<bool> SoftDeleteInvoiceAsync(int id);

        Task<string?> GetLatestInvoiceNumberForPrefixAsync(string prefix);
        Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, int? excludeId = null);
    }
}
