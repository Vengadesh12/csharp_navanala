using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IApprovalRepository _approvalRepository;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            IApprovalRepository approvalRepository)
        {
            _purchaseRepository = purchaseRepository;
            _approvalRepository = approvalRepository;
        }

        public async Task<PagedPurchaseResponse> GetPurchasesAsync(PurchaseQueryParameters query)
        {
            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 50;

            var (items, totalCount) = await _purchaseRepository.GetPurchasesPagedAsync(
                query.Status,
                query.Category,
                query.Search,
                page,
                pageSize);

            return new PagedPurchaseResponse
            {
                Data = items.Select(p => p.ToDto()).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<ApprovedProductDto>> GetApprovedProductsAsync()
        {
            var approvedApprovals = await _approvalRepository.GetApprovedApprovalsAsync();
            var purchaseGroups = await _purchaseRepository.GetPurchaseGroupsByApprovalRequestIdAsync();

            return approvedApprovals.Select(a =>
            {
                var hasQuotes = purchaseGroups.TryGetValue(a.Id, out var groupInfo);
                var count = hasQuotes ? groupInfo.Count : 0;
                var firstId = hasQuotes && groupInfo.FirstPurchaseId > 0 ? (int?)groupInfo.FirstPurchaseId : null;

                return new ApprovedProductDto
                {
                    Id = a.Id,
                    ItemName = a.ItemName,
                    Category = a.Category,
                    Quantity = a.Quantity,
                    EstimatedAmount = a.EstimatedAmount,
                    Priority = a.Priority,
                    EmployeeName = a.EmployeeName,
                    EmployeeEmail = a.EmployeeEmail,
                    DepartmentName = a.DepartmentName,
                    Description = a.Description,
                    ReviewedAt = a.ReviewedAt,
                    ReviewedByName = a.ReviewedByName,
                    HasExistingQuotation = count > 0,
                    QuotationCount = count,
                    ExistingPurchaseId = firstId
                };
            }).ToList();
        }

        public async Task<PurchaseSummaryDto> GetSummaryAsync()
        {
            var activePurchases = await _purchaseRepository.GetAllActivePurchasesAsync();
            var approvedApprovals = await _approvalRepository.GetApprovedApprovalsAsync();
            var totalApprovedCount = approvedApprovals.Count;

            var purchasesWithApprovedIdCount = activePurchases
                .Select(p => p.ApprovalRequestId)
                .Distinct()
                .Count();

            var pendingQuotationCount = Math.Max(0, totalApprovedCount - purchasesWithApprovedIdCount);

            return new PurchaseSummaryDto
            {
                TotalPurchases = activePurchases.Count,
                TotalQuotationValue = activePurchases.Sum(p => p.QuotationAmount),
                QuotationReceivedCount = activePurchases.Count(p => p.Status.Equals("Quotation Received", StringComparison.OrdinalIgnoreCase)),
                PoIssuedCount = activePurchases.Count(p => p.Status.Equals("PO Issued", StringComparison.OrdinalIgnoreCase)),
                InProcurementCount = activePurchases.Count(p => p.Status.Equals("In Procurement", StringComparison.OrdinalIgnoreCase)),
                DeliveredCount = activePurchases.Count(p => p.Status.Equals("Delivered", StringComparison.OrdinalIgnoreCase)),
                CompletedCount = activePurchases.Count(p => p.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
                ApprovedItemsPendingQuotation = pendingQuotationCount
            };
        }

        public async Task<PurchaseDto?> GetPurchaseByIdAsync(int id)
        {
            var purchase = await _purchaseRepository.GetPurchaseByIdAsync(id);
            return purchase?.ToDto();
        }

        public async Task<PurchaseDto> CreatePurchaseAsync(CreatePurchaseRequest request, int createdByUserId, string createdByName)
        {
            var approval = await _approvalRepository.GetByIdAsync(request.ApprovalRequestId);

            if (approval == null)
            {
                throw new InvalidOperationException($"Approval request #{request.ApprovalRequestId} does not exist.");
            }

            if (!approval.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Cannot create purchase quotation. Approval request status is '{approval.Status}', but must be 'Approved'.");
            }

            var now = DateTime.UtcNow;
            var purchase = new Purchase
            {
                ApprovalRequestId = approval.Id,
                ItemName = approval.ItemName.Trim(),
                Category = string.IsNullOrWhiteSpace(approval.Category) ? "Hardware & Devices" : approval.Category.Trim(),
                Quantity = approval.Quantity > 0 ? approval.Quantity : 1,
                EstimatedAmount = approval.EstimatedAmount,
                EmployeeName = approval.EmployeeName.Trim(),
                EmployeeEmail = approval.EmployeeEmail.Trim(),
                DepartmentName = string.IsNullOrWhiteSpace(approval.DepartmentName) ? null : approval.DepartmentName.Trim(),
                VendorName = request.VendorName.Trim(),
                VendorContact = request.VendorContact?.Trim(),
                VendorEmail = request.VendorEmail?.Trim(),
                QuotationNumber = request.QuotationNumber?.Trim(),
                QuotationAmount = Math.Max(0, request.QuotationAmount),
                QuotationDate = request.QuotationDate ?? now,
                DeliveryTimeline = string.IsNullOrWhiteSpace(request.DeliveryTimeline) ? "3-5 Business Days" : request.DeliveryTimeline.Trim(),
                PaymentTerms = string.IsNullOrWhiteSpace(request.PaymentTerms) ? "Net 30" : request.PaymentTerms.Trim(),
                Notes = request.Notes?.Trim(),
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Quotation Received" : request.Status.Trim(),
                CreatedByUserId = createdByUserId,
                CreatedByName = createdByName,
                CreatedAt = now,
                UpdatedAt = now,
                DeletedFlag = 1
            };

            await _purchaseRepository.AddPurchaseAsync(purchase);

            return purchase.ToDto();
        }

        public async Task<PurchaseDto?> UpdatePurchaseAsync(int id, UpdatePurchaseRequest request)
        {
            var purchase = await _purchaseRepository.GetPurchaseByIdAsync(id);
            if (purchase == null) return null;

            if (!string.IsNullOrWhiteSpace(request.VendorName)) purchase.VendorName = request.VendorName.Trim();
            purchase.VendorContact = request.VendorContact?.Trim();
            purchase.VendorEmail = request.VendorEmail?.Trim();
            purchase.QuotationNumber = request.QuotationNumber?.Trim();
            purchase.QuotationAmount = Math.Max(0, request.QuotationAmount);
            if (request.QuotationDate.HasValue) purchase.QuotationDate = request.QuotationDate.Value;
            if (!string.IsNullOrWhiteSpace(request.DeliveryTimeline)) purchase.DeliveryTimeline = request.DeliveryTimeline.Trim();
            if (!string.IsNullOrWhiteSpace(request.PaymentTerms)) purchase.PaymentTerms = request.PaymentTerms.Trim();
            purchase.Notes = request.Notes?.Trim();
            if (!string.IsNullOrWhiteSpace(request.Status)) purchase.Status = request.Status.Trim();
            purchase.UpdatedAt = DateTime.UtcNow;

            await _purchaseRepository.UpdatePurchaseAsync(purchase);

            return purchase.ToDto();
        }

        public async Task<bool> DeletePurchaseAsync(int id)
        {
            return await _purchaseRepository.SoftDeletePurchaseAsync(id);
        }
    }
}
