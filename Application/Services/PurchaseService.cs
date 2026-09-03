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
    public class PurchaseService : IPurchaseService
    {
        private readonly IApplicationDbContext _context;

        public PurchaseService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedPurchaseResponse> GetPurchasesAsync(PurchaseQueryParameters query)
        {
            var dbQuery = _context.Purchases.AsNoTracking().Where(p => p.DeletedFlag == 1);

            if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var statusLower = query.Status.Trim().ToLower();
                dbQuery = dbQuery.Where(p => p.Status.ToLower() == statusLower);
            }

            if (!string.IsNullOrWhiteSpace(query.Category) && !query.Category.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var categoryLower = query.Category.Trim().ToLower();
                dbQuery = dbQuery.Where(p => p.Category.ToLower() == categoryLower);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(p =>
                    p.ItemName.ToLower().Contains(search) ||
                    p.VendorName.ToLower().Contains(search) ||
                    (p.VendorContact != null && p.VendorContact.ToLower().Contains(search)) ||
                    (p.VendorEmail != null && p.VendorEmail.ToLower().Contains(search)) ||
                    (p.QuotationNumber != null && p.QuotationNumber.ToLower().Contains(search)) ||
                    p.EmployeeName.ToLower().Contains(search) ||
                    (p.DepartmentName != null && p.DepartmentName.ToLower().Contains(search)));
            }

            var totalCount = await dbQuery.CountAsync();
            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 50;

            var rawPurchases = await dbQuery
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = rawPurchases.Select(p => p.ToDto()).ToList();

            return new PagedPurchaseResponse
            {
                Data = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<ApprovedProductDto>> GetApprovedProductsAsync()
        {
            // Only items with status 'Approved' and deleted_flag == 1
            var approvedApprovals = await _context.Approvals
                .AsNoTracking()
                .Where(a => a.DeletedFlag == 1 && a.Status.ToLower() == "approved")
                .OrderByDescending(a => a.ReviewedAt ?? a.CreatedAt)
                .ToListAsync();

            var purchaseGroups = await _context.Purchases
                .AsNoTracking()
                .Where(p => p.DeletedFlag == 1)
                .GroupBy(p => p.ApprovalRequestId)
                .Select(g => new
                {
                    ApprovalRequestId = g.Key,
                    Count = g.Count(),
                    FirstPurchaseId = g.OrderBy(p => p.Id).Select(p => p.Id).FirstOrDefault()
                })
                .ToDictionaryAsync(g => g.ApprovalRequestId);

            return approvedApprovals.Select(a =>
            {
                var hasQuotes = purchaseGroups.TryGetValue(a.Id, out var groupInfo);
                var count = hasQuotes && groupInfo != null ? groupInfo.Count : 0;
                var firstId = hasQuotes && groupInfo != null && groupInfo.FirstPurchaseId > 0 ? (int?)groupInfo.FirstPurchaseId : null;

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
            var activePurchases = await _context.Purchases
                .AsNoTracking()
                .Where(p => p.DeletedFlag == 1)
                .ToListAsync();

            var totalApprovedCount = await _context.Approvals
                .AsNoTracking()
                .CountAsync(a => a.DeletedFlag == 1 && a.Status.ToLower() == "approved");

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
            var purchase = await _context.Purchases
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedFlag == 1);

            return purchase?.ToDto();
        }

        public async Task<PurchaseDto> CreatePurchaseAsync(CreatePurchaseRequest request, int createdByUserId, string createdByName)
        {
            var approval = await _context.Approvals
                .FirstOrDefaultAsync(a => a.Id == request.ApprovalRequestId && a.DeletedFlag == 1);

            if (approval == null)
            {
                throw new InvalidOperationException($"Approval request #{request.ApprovalRequestId} does not exist.");
            }

            if (!approval.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Cannot create purchase quotation. Approval request status is '{approval.Status}', but must be 'Approved'.");
            }

            var purchase = Purchase.CreateFromApproval(
                approvalRequestId: approval.Id,
                itemName: approval.ItemName,
                category: approval.Category,
                quantity: approval.Quantity,
                estimatedAmount: approval.EstimatedAmount,
                employeeName: approval.EmployeeName,
                employeeEmail: approval.EmployeeEmail,
                departmentName: approval.DepartmentName,
                vendorName: request.VendorName,
                vendorContact: request.VendorContact,
                vendorEmail: request.VendorEmail,
                quotationNumber: request.QuotationNumber,
                quotationAmount: request.QuotationAmount,
                quotationDate: request.QuotationDate,
                deliveryTimeline: request.DeliveryTimeline,
                paymentTerms: request.PaymentTerms,
                notes: request.Notes,
                status: request.Status,
                createdByUserId: createdByUserId,
                createdByName: createdByName
            );

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return purchase.ToDto();
        }

        public async Task<PurchaseDto?> UpdatePurchaseAsync(int id, UpdatePurchaseRequest request)
        {
            var purchase = await _context.Purchases
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedFlag == 1);

            if (purchase == null) return null;

            purchase.UpdateQuotation(
                vendorName: request.VendorName,
                vendorContact: request.VendorContact,
                vendorEmail: request.VendorEmail,
                quotationNumber: request.QuotationNumber,
                quotationAmount: request.QuotationAmount,
                quotationDate: request.QuotationDate,
                deliveryTimeline: request.DeliveryTimeline,
                paymentTerms: request.PaymentTerms,
                notes: request.Notes,
                status: request.Status
            );

            await _context.SaveChangesAsync();

            return purchase.ToDto();
        }

        public async Task<bool> DeletePurchaseAsync(int id)
        {
            var purchase = await _context.Purchases
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedFlag == 1);

            if (purchase == null) return false;

            purchase.SoftDelete();

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
