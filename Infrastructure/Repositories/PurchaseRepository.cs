using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;
using MyBackend.Infrastructure.Persistence;

namespace MyBackend.Infrastructure.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly AppDbContext _context;

        public PurchaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Purchase> Items, int TotalCount)> GetPurchasesPagedAsync(string? status, string? category, string? search, int page, int pageSize)
        {
            var dbQuery = _context.Purchases.AsNoTracking().Where(p => p.DeletedFlag == 1);

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var statusLower = status.Trim().ToLower();
                dbQuery = dbQuery.Where(p => p.Status.ToLower() == statusLower);
            }

            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var categoryLower = category.Trim().ToLower();
                dbQuery = dbQuery.Where(p => p.Category.ToLower() == categoryLower);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                dbQuery = dbQuery.Where(p =>
                    p.ItemName.ToLower().Contains(s) ||
                    p.VendorName.ToLower().Contains(s) ||
                    (p.VendorContact != null && p.VendorContact.ToLower().Contains(s)) ||
                    (p.VendorEmail != null && p.VendorEmail.ToLower().Contains(s)) ||
                    (p.QuotationNumber != null && p.QuotationNumber.ToLower().Contains(s)) ||
                    p.EmployeeName.ToLower().Contains(s) ||
                    (p.DepartmentName != null && p.DepartmentName.ToLower().Contains(s)));
            }

            var totalCount = await dbQuery.CountAsync();
            var pageNum = page > 0 ? page : 1;
            var size = pageSize > 0 ? pageSize : 50;

            var items = await dbQuery
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNum - 1) * size)
                .Take(size)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Dictionary<int, (int Count, int FirstPurchaseId)>> GetPurchaseGroupsByApprovalRequestIdAsync()
        {
            var groups = await _context.Purchases
                .AsNoTracking()
                .Where(p => p.DeletedFlag == 1)
                .GroupBy(p => p.ApprovalRequestId)
                .Select(g => new
                {
                    ApprovalRequestId = g.Key,
                    Count = g.Count(),
                    FirstPurchaseId = g.OrderBy(p => p.Id).Select(p => p.Id).FirstOrDefault()
                })
                .ToListAsync();

            return groups.ToDictionary(g => g.ApprovalRequestId, g => (g.Count, g.FirstPurchaseId));
        }

        public async Task<Purchase?> GetPurchaseByIdAsync(int id)
        {
            return await _context.Purchases
                .FirstOrDefaultAsync(p => p.Id == id && p.DeletedFlag == 1);
        }

        public async Task<List<Purchase>> GetAllActivePurchasesAsync()
        {
            return await _context.Purchases
                .AsNoTracking()
                .Where(p => p.DeletedFlag == 1)
                .ToListAsync();
        }

        public async Task<Purchase> AddPurchaseAsync(Purchase purchase)
        {
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();
            return purchase;
        }

        public async Task UpdatePurchaseAsync(Purchase purchase)
        {
            _context.Purchases.Update(purchase);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SoftDeletePurchaseAsync(int id)
        {
            var purchase = await _context.Purchases.FirstOrDefaultAsync(p => p.Id == id && p.DeletedFlag == 1);
            if (purchase == null) return false;

            purchase.SoftDelete();
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
