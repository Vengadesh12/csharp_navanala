using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;
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

        public async Task<(List<SystemSetting> Settings, List<SettingCategory> Categories, Dictionary<string, int> SettingCounts, int TotalSettings, string? TwoFactorValue, int AlertChannels, string? SessionTimeout)> GetSettingsOverviewDataAsync(string? category, string? search)
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

            var categoriesList = await _context.SettingCategories
                .FromSqlRaw("""
                    SELECT id, name, description, icon, created_at, updated_at, created_by, deleted_flag
                    FROM setting_categories
                    WHERE deleted_flag = 1 AND LOWER(name) IN ('general', 'security')
                    ORDER BY id ASC
                """)
                .AsNoTracking()
                .ToListAsync();

            var settingCounts = (await _context.SystemSettings
                .FromSqlRaw("""
                    SELECT id, setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by
                    FROM system_settings
                    WHERE LOWER(category) IN ('general', 'security')
                """)
                .AsNoTracking()
                .ToListAsync())
                .GroupBy(s => s.Category)
                .ToDictionary(g => g.Key.ToLower(), g => g.Count());

            var totalSettings = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM system_settings
                WHERE LOWER(category) IN ('general', 'security')
            """).SingleOrDefaultAsync();

            var twoFactorVal = await _context.Database.SqlQueryRaw<string>("""
                SELECT setting_value AS "Value"
                FROM system_settings
                WHERE setting_key = 'two_factor_auth'
            """).FirstOrDefaultAsync();

            var alertChannels = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM setting_categories
                WHERE deleted_flag = 1 AND LOWER(name) = 'security'
            """).SingleOrDefaultAsync();

            var sessionTimeoutVal = await _context.Database.SqlQueryRaw<string>("""
                SELECT setting_value AS "Value"
                FROM system_settings
                WHERE setting_key = 'session_timeout'
            """).FirstOrDefaultAsync();

            return (rawSettings, categoriesList, settingCounts, totalSettings, twoFactorVal, alertChannels, sessionTimeoutVal);
        }

        public async Task<(List<SettingCategory> Categories, Dictionary<string, int> SettingCounts)> GetCategoriesWithCountsAsync()
        {
            var categories = await _context.SettingCategories
                .FromSqlRaw("""
                    SELECT id, name, description, icon, created_at, updated_at, created_by, deleted_flag
                    FROM setting_categories
                    WHERE deleted_flag = 1 AND LOWER(name) IN ('general', 'security')
                    ORDER BY id ASC
                """)
                .AsNoTracking()
                .ToListAsync();

            var settingCounts = (await _context.SystemSettings
                .FromSqlRaw("""
                    SELECT id, setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by
                    FROM system_settings
                    WHERE LOWER(category) IN ('general', 'security')
                """)
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
                var count = await _context.Database.SqlQueryRaw<int>("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM setting_categories
                    WHERE id <> {0} AND deleted_flag = 1 AND LOWER(name) = LOWER({1})
                """, excludeId.Value, trimmed).SingleOrDefaultAsync();
                return count > 0;
            }
            else
            {
                var count = await _context.Database.SqlQueryRaw<int>("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM setting_categories
                    WHERE deleted_flag = 1 AND LOWER(name) = LOWER({0})
                """, trimmed).SingleOrDefaultAsync();
                return count > 0;
            }
        }

        public async Task<int> CreateCategoryAsync(string name, string description, string icon, string createdBy)
        {
            var now = DateTime.UtcNow;
            return await _context.Database.SqlQueryRaw<int>("""
                INSERT INTO setting_categories (name, description, icon, created_at, updated_at, created_by, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {3}, {4}, 1)
                RETURNING id AS "Value"
            """, name, description, icon, now, createdBy).SingleAsync();
        }

        public async Task<SettingCategory?> GetCategoryByIdAsync(int id)
        {
            return await _context.SettingCategories
                .FromSqlRaw("""
                    SELECT id, name, description, icon, created_at, updated_at, created_by, deleted_flag
                    FROM setting_categories
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetCategorySettingCountAsync(string categoryName)
        {
            return await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM system_settings
                WHERE LOWER(category) = LOWER({0})
            """, categoryName).SingleOrDefaultAsync();
        }

        public async Task<bool> UpdateCategoryAsync(int id, string name, string description, string icon)
        {
            var now = DateTime.UtcNow;
            var rows = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE setting_categories
                SET name = {0}, description = {1}, icon = {2}, updated_at = {3}
                WHERE id = {4} AND deleted_flag = 1
            """, name, description, icon, now, id);

            return rows > 0;
        }

        public async Task<bool> SoftDeleteCategoryAsync(int id)
        {
            var now = DateTime.UtcNow;
            var rows = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE setting_categories
                SET deleted_flag = 0, updated_at = {0}
                WHERE id = {1} AND deleted_flag = 1
            """, now, id);

            return rows > 0;
        }

        public async Task<bool> BulkUpdateSettingsAsync(IDictionary<string, string> settings, string updatedBy)
        {
            var now = DateTime.UtcNow;

            foreach (var kvp in settings)
            {
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                    UPDATE system_settings
                    SET setting_value = {0}, updated_at = {1}, updated_by = {2}
                    WHERE setting_key = {3}
                """, kvp.Value, now, updatedBy, kvp.Key);

                if (rowsAffected == 0)
                {
                    await _context.Database.ExecuteSqlRawAsync("""
                        INSERT INTO system_settings (setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by)
                        VALUES ({0}, {1}, 'General', {2}, 'string', {3}, {3}, {4})
                    """, kvp.Key, kvp.Value, kvp.Key, now, updatedBy);
                }
            }

            return true;
        }

        public async Task<bool> SettingExistsByKeyAsync(string key)
        {
            var count = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM system_settings
                WHERE setting_key = {0}
            """, key).SingleOrDefaultAsync();

            return count > 0;
        }

        public async Task<int> CreateSettingAsync(string key, string value, string category, string description, string dataType, string createdBy)
        {
            var now = DateTime.UtcNow;
            return await _context.Database.SqlQueryRaw<int>("""
                INSERT INTO system_settings (setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {5}, {6})
                RETURNING id AS "Value"
            """, key, value, category, description, dataType, now, createdBy).SingleAsync();
        }

        public async Task<SystemSetting?> GetSettingByIdAsync(int id)
        {
            return await _context.SystemSettings
                .FromSqlRaw("""
                    SELECT id, setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by
                    FROM system_settings
                    WHERE id = {0}
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateSettingAsync(int id, string key, string value, string category, string description, string dataType, string updatedBy)
        {
            var now = DateTime.UtcNow;
            var rows = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE system_settings
                SET setting_key = {0}, setting_value = {1}, category = {2}, description = {3}, data_type = {4}, updated_at = {5}, updated_by = {6}
                WHERE id = {7}
            """, key, value, category, description, dataType, now, updatedBy, id);

            return rows > 0;
        }

        public async Task<bool> DeleteSettingAsync(int id)
        {
            var rows = await _context.Database.ExecuteSqlRawAsync("""
                DELETE FROM system_settings
                WHERE id = {0}
            """, id);

            return rows > 0;
        }

        public async Task<string?> GetSettingValueAsync(string key)
        {
            return await _context.Database.SqlQueryRaw<string>("""
                SELECT setting_value AS "Value"
                FROM system_settings
                WHERE setting_key = {0}
            """, key).FirstOrDefaultAsync();
        }
    }
}
