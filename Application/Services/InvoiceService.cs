using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private const string DEFAULT_COMPANY_GSTIN = "36AAAAA0000A1Z5";

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<PagedInvoiceResponse> GetInvoicesAsync(InvoiceQueryParameters query)
        {
            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 50;

            var (items, totalCount) = await _invoiceRepository.GetInvoicesPagedAsync(
                query.Status,
                query.StartDate,
                query.EndDate,
                query.Search,
                page,
                pageSize);

            return new PagedInvoiceResponse
            {
                Success = true,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Data = items.Select(i => i.ToDto()).ToList()
            };
        }

        public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
        {
            var invoice = await _invoiceRepository.GetInvoiceByIdAsync(id);
            return invoice?.ToDto();
        }

        public async Task<InvoiceSummaryDto> GetSummaryAsync()
        {
            var (totalInvoices, totalInvoicedAmount, totalPaidAmount, totalPendingAmount, totalGstCollected, paidCount, pendingCount, draftCount, overdueCount) =
                await _invoiceRepository.GetSummaryAsync();

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
            var invoiceNumber = string.IsNullOrWhiteSpace(request.InvoiceNumber)
                ? await GenerateInvoiceNumberAsync()
                : request.InvoiceNumber.Trim();

            var companyGstin = (canEditGst && !string.IsNullOrWhiteSpace(request.CompanyGstin))
                ? request.CompanyGstin.Trim().ToUpper()
                : DEFAULT_COMPANY_GSTIN;

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

            await _invoiceRepository.AddInvoiceAsync(invoice);

            return invoice.ToDto();
        }

        public async Task<InvoiceDto?> UpdateInvoiceAsync(int id, UpdateInvoiceRequest request, int userId, string userName, bool canEditGst)
        {
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
                    invoiceId: id
                ));
            }

            var companyGstinToUse = (canEditGst && !string.IsNullOrWhiteSpace(request.CompanyGstin))
                ? request.CompanyGstin.Trim().ToUpper()
                : null;

            var updatedInvoice = await _invoiceRepository.UpdateInvoiceWithItemsAsync(
                id,
                request.InvoiceNumber,
                request.CustomerName,
                request.CustomerEmail,
                request.CustomerPhone,
                request.CustomerAddress,
                request.CustomerGstin,
                companyGstinToUse,
                request.InvoiceDate,
                request.DueDate,
                request.DiscountAmount,
                request.Status,
                request.PaymentMethod,
                request.Notes,
                request.TermsAndConditions,
                lineItems
            );

            return updatedInvoice?.ToDto();
        }

        public async Task<bool> DeleteInvoiceAsync(int id, int userId)
        {
            return await _invoiceRepository.SoftDeleteInvoiceAsync(id);
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"INV-{year}-";
            var latest = await _invoiceRepository.GetLatestInvoiceNumberForPrefixAsync(prefix);

            int nextNum = 1;
            if (!string.IsNullOrWhiteSpace(latest))
            {
                var suffix = latest.Replace(prefix, "");
                if (int.TryParse(suffix, out int parsed))
                {
                    nextNum = parsed + 1;
                }
            }

            return $"{prefix}{nextNum:D4}";
        }
    }
}
