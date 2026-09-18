using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Models;
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

        private const string BasePurchasesSelectSql = """
            SELECT id, approval_request_id, item_name, category, quantity, estimated_amount, employee_name, employee_email, department_name, vendor_name, vendor_contact, vendor_email, quotation_number, quotation_amount, quotation_date, delivery_timeline, payment_terms, notes, status, created_by_user_id, created_by_name, created_at, updated_at, deleted_flag
            FROM purchases
        """;

        public async Task<(List<PurchaseModel> Items, int TotalCount)> GetPurchasesPagedAsync(string? status, string? category, string? search, int page, int pageSize)
        {
            var sql = new StringBuilder(BasePurchasesSelectSql)
                .AppendLine(" WHERE deleted_flag = 1");

            var countSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM purchases
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

            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                var clause = $" AND LOWER(category) = LOWER({{{paramIndex++}}})";
                sql.Append(clause);
                countSql.Append(clause);
                parameters.Add(category.Trim());
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim().ToLower()}%";
                var clause = $" AND (LOWER(item_name) LIKE {{{paramIndex}}} OR LOWER(vendor_name) LIKE {{{paramIndex}}} OR (vendor_contact IS NOT NULL AND LOWER(vendor_contact) LIKE {{{paramIndex}}}) OR (vendor_email IS NOT NULL AND LOWER(vendor_email) LIKE {{{paramIndex}}}) OR (quotation_number IS NOT NULL AND LOWER(quotation_number) LIKE {{{paramIndex}}}) OR LOWER(employee_name) LIKE {{{paramIndex}}} OR (department_name IS NOT NULL AND LOWER(department_name) LIKE {{{paramIndex++}}}))";
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

            var items = await _context.Purchases
                .FromSqlRaw(sql.ToString(), parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Dictionary<int, (int Count, int FirstPurchaseId)>> GetPurchaseGroupsByApprovalRequestIdAsync()
        {
            var sql = new StringBuilder(BasePurchasesSelectSql)
                .AppendLine(" WHERE deleted_flag = 1");

            var purchases = await _context.Purchases
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .ToListAsync();

            var groups = purchases
                .GroupBy(p => p.ApprovalRequestId)
                .Select(g => new
                {
                    ApprovalRequestId = g.Key,
                    Count = g.Count(),
                    FirstPurchaseId = g.OrderBy(p => p.Id).Select(p => p.Id).FirstOrDefault()
                })
                .ToDictionary(g => g.ApprovalRequestId, g => (g.Count, g.FirstPurchaseId));

            return groups;
        }

        public async Task<PurchaseModel?> GetPurchaseByIdAsync(int id)
        {
            var sql = new StringBuilder(BasePurchasesSelectSql)
                .AppendLine(" WHERE id = {0} AND deleted_flag = 1 LIMIT 1");

            return await _context.Purchases
                .FromSqlRaw(sql.ToString(), id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<List<PurchaseModel>> GetAllActivePurchasesAsync()
        {
            var sql = new StringBuilder(BasePurchasesSelectSql)
                .AppendLine(" WHERE deleted_flag = 1 ORDER BY created_at DESC");

            return await _context.Purchases
                .FromSqlRaw(sql.ToString())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PurchaseModel> AddPurchaseAsync(PurchaseModel purchase)
        {
            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();
            return purchase;
        }

        public async Task UpdatePurchaseAsync(PurchaseModel purchase)
        {
            _context.Purchases.Update(purchase);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SoftDeletePurchaseAsync(int id)
        {
            var purchase = await _context.Purchases.FirstOrDefaultAsync(p => p.Id == id && p.DeletedFlag == 1);
            if (purchase == null) return false;

            purchase.DeletedFlag = 0;
            purchase.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
