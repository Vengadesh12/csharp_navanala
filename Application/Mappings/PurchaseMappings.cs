using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.DTO;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Mappings
{
    public static class PurchaseMappings
    {
        public static PurchaseDto ToDto(this Purchase entity)
        {
            return new PurchaseDto
            {
                Id = entity.Id,
                ApprovalRequestId = entity.ApprovalRequestId,
                ItemName = entity.ItemName,
                Category = entity.Category,
                Quantity = entity.Quantity,
                EstimatedAmount = entity.EstimatedAmount,
                EmployeeName = entity.EmployeeName,
                EmployeeEmail = entity.EmployeeEmail,
                DepartmentName = entity.DepartmentName,
                VendorName = entity.VendorName,
                VendorContact = entity.VendorContact,
                VendorEmail = entity.VendorEmail,
                QuotationNumber = entity.QuotationNumber,
                QuotationAmount = entity.QuotationAmount,
                QuotationDate = entity.QuotationDate,
                DeliveryTimeline = entity.DeliveryTimeline,
                PaymentTerms = entity.PaymentTerms,
                Notes = entity.Notes,
                Status = entity.Status,
                CreatedByUserId = entity.CreatedByUserId,
                CreatedByName = entity.CreatedByName,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static List<PurchaseDto> ToDtoList(this IEnumerable<Purchase> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }

        public static ApprovedProductDto ToApprovedProductDto(this ApprovalRequest entity, int quotationCount = 0, int? existingPurchaseId = null)
        {
            return new ApprovedProductDto
            {
                Id = entity.Id,
                ItemName = entity.ItemName,
                Category = entity.Category,
                Quantity = entity.Quantity,
                EstimatedAmount = entity.EstimatedAmount,
                Priority = entity.Priority,
                EmployeeName = entity.EmployeeName,
                EmployeeEmail = entity.EmployeeEmail,
                DepartmentName = entity.DepartmentName,
                Description = entity.Description,
                ReviewedAt = entity.ReviewedAt,
                ReviewedByName = entity.ReviewedByName,
                HasExistingQuotation = quotationCount > 0,
                QuotationCount = quotationCount,
                ExistingPurchaseId = existingPurchaseId
            };
        }
    }
}
