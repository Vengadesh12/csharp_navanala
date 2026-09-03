using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.DTO;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Mappings
{
    public static class InvoiceMappings
    {
        public static InvoiceItemDto ToDto(this InvoiceItem item)
        {
            return new InvoiceItemDto
            {
                Id = item.Id,
                InvoiceId = item.InvoiceId,
                ProductName = item.ProductName,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                TaxAmount = item.TaxAmount,
                TotalAmount = item.TotalAmount,
                OrderIndex = item.OrderIndex
            };
        }

        public static InvoiceDto ToDto(this Invoice invoice, string? createdByName = null)
        {
            return new InvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                CustomerName = invoice.CustomerName,
                CustomerEmail = invoice.CustomerEmail,
                CustomerPhone = invoice.CustomerPhone,
                CustomerAddress = invoice.CustomerAddress,
                CustomerGstin = invoice.CustomerGstin,
                CompanyGstin = invoice.CompanyGstin,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                Subtotal = invoice.Subtotal,
                TaxRate = invoice.TaxRate,
                TaxAmount = invoice.TaxAmount,
                DiscountAmount = invoice.DiscountAmount,
                TotalAmount = invoice.TotalAmount,
                TotalAmountInWords = invoice.TotalAmountInWords,
                Status = invoice.Status,
                PaymentMethod = invoice.PaymentMethod,
                Notes = invoice.Notes,
                TermsAndConditions = invoice.TermsAndConditions,
                CreatedByUserId = invoice.CreatedByUserId,
                CreatedByName = createdByName ?? invoice.CreatedByName,
                CreatedAt = invoice.CreatedAt,
                UpdatedAt = invoice.UpdatedAt,
                Items = invoice.Items != null ? invoice.Items.Select(i => i.ToDto()).ToList() : new List<InvoiceItemDto>()
            };
        }

        public static List<InvoiceDto> ToDtoList(this IEnumerable<Invoice> invoices, IReadOnlyDictionary<int, string>? userNames = null)
        {
            return invoices.Select(inv =>
            {
                string? name = null;
                if (userNames != null && userNames.TryGetValue(inv.CreatedByUserId, out var n))
                {
                    name = n;
                }
                return inv.ToDto(name);
            }).ToList();
        }
    }
}
