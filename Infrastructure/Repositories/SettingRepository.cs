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
    public class SettingRepository : ISettingRepository
    {
        private readonly AppDbContext _context;

        public SettingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<SystemSettingModel> Settings, List<SettingCategoryModel> Categories, Dictionary<string, int> SettingCounts, int TotalSettings, string? TwoFactorValue, int AlertChannels, string? SessionTimeout)> GetSettingsOverviewDataAsync(string? category, string? search)
        {
            var sql = new StringBuilder("""
                SELECT id, setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by
                FROM system_settings
                WHERE 1=1
            """);

            var parameters = new List<object>();
            int paramIndex = 0;

            if (!string.IsNullOrWhiteSpace(category) && category != "ALL")
            {
                sql.Append($" AND LOWER(category) = LOWER({{{paramIndex++}}})");
                parameters.Add(category.Trim());
            }
            else
            {
                sql.Append(" AND LOWER(category) IN ('general', 'security')");
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var pattern = $"%{search.Trim().ToLower()}%";
                sql.Append($" AND (LOWER(setting_key) LIKE {{{paramIndex}}} OR LOWER(description) LIKE {{{paramIndex}}} OR LOWER(category) LIKE {{{paramIndex++}}})");
                parameters.Add(pattern);
            }

            sql.Append(" ORDER BY category ASC, setting_key ASC");

            var rawSettings = await _context.SystemSettings
                .FromSqlRaw(sql.ToString(), parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            var categoriesSql = new StringBuilder("""
                SELECT id, name, description, icon, created_at, updated_at, created_by, deleted_flag
                FROM setting_categories
                WHERE deleted_flag = 1 AND LOWER(name) IN ('general', 'security')
                ORDER BY id ASC
            """);

            var categoriesList = await _context.SettingCategories
                .FromSqlRaw(categoriesSql.ToString())
                .AsNoTracking()
                .ToListAsync();

            var settingCountsSql = new StringBuilder("""
                SELECT id, setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by
                FROM system_settings
                WHERE LOWER(category) IN ('general', 'security')
            """);

            var settingCounts = (await _context.SystemSettings
                .FromSqlRaw(settingCountsSql.ToString())
                .AsNoTracking()
                .ToListAsync())
                .GroupBy(s => s.Category)
                .ToDictionary(g => g.Key.ToLower(), g => g.Count());

            var totalSettingsSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM system_settings
                WHERE LOWER(category) IN ('general', 'security')
            """);

            var totalSettings = await _context.Database.SqlQueryRaw<int>(totalSettingsSql.ToString()).SingleOrDefaultAsync();

            var twoFactorSql = new StringBuilder("""
                SELECT setting_value AS "Value"
                FROM system_settings
                WHERE setting_key = 'two_factor_auth'
            """);

            var twoFactorVal = await _context.Database.SqlQueryRaw<string>(twoFactorSql.ToString()).FirstOrDefaultAsync();

            var alertChannelsSql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM setting_categories
                WHERE deleted_flag = 1 AND LOWER(name) = 'security'
            """);

            var alertChannels = await _context.Database.SqlQueryRaw<int>(alertChannelsSql.ToString()).SingleOrDefaultAsync();

            var sessionTimeoutSql = new StringBuilder("""
                SELECT setting_value AS "Value"
                FROM system_settings
                WHERE setting_key = 'session_timeout'
            """);

            var sessionTimeoutVal = await _context.Database.SqlQueryRaw<string>(sessionTimeoutSql.ToString()).FirstOrDefaultAsync();

            return (rawSettings, categoriesList, settingCounts, totalSettings, twoFactorVal, alertChannels, sessionTimeoutVal);
        }

        public async Task<(List<SettingCategoryModel> Categories, Dictionary<string, int> SettingCounts)> GetCategoriesWithCountsAsync()
        {
            var categoriesSql = new StringBuilder("""
                SELECT id, name, description, icon, created_at, updated_at, created_by, deleted_flag
                FROM setting_categories
                WHERE deleted_flag = 1 AND LOWER(name) IN ('general', 'security')
                ORDER BY id ASC
            """);

            var categories = await _context.SettingCategories
                .FromSqlRaw(categoriesSql.ToString())
                .AsNoTracking()
                .ToListAsync();

            var settingCountsSql = new StringBuilder("""
                SELECT id, setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by
                FROM system_settings
                WHERE LOWER(category) IN ('general', 'security')
            """);

            var settingCounts = (await _context.SystemSettings
                .FromSqlRaw(settingCountsSql.ToString())
                .AsNoTracking()
                .ToListAsync())
                .GroupBy(s => s.Category)
                .ToDictionary(g => g.Key.ToLower(), g => g.Count());

            return (categories, settingCounts);
        }

        public async Task<bool> CategoryExistsByNameAsync(string name, int? excludeId = null)
        {
            var trimmed = name.Trim();
            if (excludeId.HasValue)
            {
                var sql = new StringBuilder("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM setting_categories
                    WHERE id <> {0} AND deleted_flag = 1 AND LOWER(name) = LOWER({1})
                """);

                var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), excludeId.Value, trimmed).SingleOrDefaultAsync();
                return count > 0;
            }
            else
            {
                var sql = new StringBuilder("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM setting_categories
                    WHERE deleted_flag = 1 AND LOWER(name) = LOWER({0})
                """);

                var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), trimmed).SingleOrDefaultAsync();
                return count > 0;
            }
        }

        public Task<int> CreateCategoryAsync(string name, string description, string icon, string createdBy)
        {
            var now = DateTime.UtcNow;
            var insertSql = new StringBuilder("""
                INSERT INTO setting_categories (name, description, icon, created_at, updated_at, created_by, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {3}, {4}, 1)
                RETURNING id AS "Value"
            """);

            var id = _context.Database.SqlQueryRaw<int>(
                insertSql.ToString(),
                name, description, icon, now, createdBy)
            .AsEnumerable()
            .Single();

            return Task.FromResult(id);
        }

        public async Task<SettingCategoryModel?> GetCategoryByIdAsync(int id)
        {
            var sql = new StringBuilder("""
                SELECT id, name, description, icon, created_at, updated_at, created_by, deleted_flag
                FROM setting_categories
                WHERE id = {0} AND deleted_flag = 1
            """);

            return await _context.SettingCategories
                .FromSqlRaw(sql.ToString(), id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetCategorySettingCountAsync(string categoryName)
        {
            var sql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM system_settings
                WHERE LOWER(category) = LOWER({0})
            """);

            return await _context.Database.SqlQueryRaw<int>(sql.ToString(), categoryName).SingleOrDefaultAsync();
        }

        public async Task<bool> UpdateCategoryAsync(int id, string name, string description, string icon)
        {
            var now = DateTime.UtcNow;
            var updateSql = new StringBuilder("""
                UPDATE setting_categories
                SET name = {0}, description = {1}, icon = {2}, updated_at = {3}
                WHERE id = {4} AND deleted_flag = 1
            """);

            var rows = await _context.Database.ExecuteSqlRawAsync(
                updateSql.ToString(),
                name, description, icon, now, id);

            return rows > 0;
        }

        public async Task<bool> SoftDeleteCategoryAsync(int id)
        {
            var now = DateTime.UtcNow;
            var deleteSql = new StringBuilder("""
                UPDATE setting_categories
                SET deleted_flag = 0, updated_at = {0}
                WHERE id = {1} AND deleted_flag = 1
            """);

            var rows = await _context.Database.ExecuteSqlRawAsync(deleteSql.ToString(), now, id);

            return rows > 0;
        }

        public async Task<bool> BulkUpdateSettingsAsync(IDictionary<string, string> settings, string updatedBy)
        {
            var now = DateTime.UtcNow;

            var updateSql = new StringBuilder("""
                UPDATE system_settings
                SET setting_value = {0}, updated_at = {1}, updated_by = {2}
                WHERE setting_key = {3}
            """);

            var insertSql = new StringBuilder("""
                INSERT INTO system_settings (setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by)
                VALUES ({0}, {1}, 'General', {2}, 'string', {3}, {3}, {4})
            """);

            foreach (var kvp in settings)
            {
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    updateSql.ToString(),
                    kvp.Value, now, updatedBy, kvp.Key);

                if (rowsAffected == 0)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        insertSql.ToString(),
                        kvp.Key, kvp.Value, kvp.Key, now, updatedBy);
                }
            }

            return true;
        }

        public async Task<bool> SettingExistsByKeyAsync(string key)
        {
            var sql = new StringBuilder("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM system_settings
                WHERE setting_key = {0}
            """);

            var count = await _context.Database.SqlQueryRaw<int>(sql.ToString(), key).SingleOrDefaultAsync();

            return count > 0;
        }

        public Task<int> CreateSettingAsync(string key, string value, string category, string description, string dataType, string createdBy)
        {
            var now = DateTime.UtcNow;
            var insertSql = new StringBuilder("""
                INSERT INTO system_settings (setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {5}, {6})
                RETURNING id AS "Value"
            """);

            var id = _context.Database.SqlQueryRaw<int>(
                insertSql.ToString(),
                key, value, category, description, dataType, now, createdBy)
            .AsEnumerable()
            .Single();

            return Task.FromResult(id);
        }

        public async Task<SystemSettingModel?> GetSettingByIdAsync(int id)
        {
            var sql = new StringBuilder("""
                SELECT id, setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by
                FROM system_settings
                WHERE id = {0}
            """);

            return await _context.SystemSettings
                .FromSqlRaw(sql.ToString(), id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateSettingAsync(int id, string key, string value, string category, string description, string dataType, string updatedBy)
        {
            var now = DateTime.UtcNow;
            var updateSql = new StringBuilder("""
                UPDATE system_settings
                SET setting_key = {0}, setting_value = {1}, category = {2}, description = {3}, data_type = {4}, updated_at = {5}, updated_by = {6}
                WHERE id = {7}
            """);

            var rows = await _context.Database.ExecuteSqlRawAsync(
                updateSql.ToString(),
                key, value, category, description, dataType, now, updatedBy, id);

            return rows > 0;
        }

        public async Task<bool> DeleteSettingAsync(int id)
        {
            var deleteSql = new StringBuilder("""
                DELETE FROM system_settings
                WHERE id = {0}
            """);

            var rows = await _context.Database.ExecuteSqlRawAsync(deleteSql.ToString(), id);

            return rows > 0;
        }

        public async Task<string?> GetSettingValueAsync(string key)
        {
            var sql = new StringBuilder("""
                SELECT setting_value AS "Value"
                FROM system_settings
                WHERE setting_key = {0}
            """);

            return await _context.Database.SqlQueryRaw<string>(sql.ToString(), key).FirstOrDefaultAsync();
        }
    }
}
