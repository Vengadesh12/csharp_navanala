using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;

namespace MyBackend.Application.Services
{
    public class SettingService : ISettingService
    {
        private readonly IApplicationDbContext _context;

        public SettingService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SettingsOverviewResponse> GetSettingsAsync(string? category, string? search)
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

            var categoryDtos = categoriesList.Select(c => c.ToDto(settingCounts.TryGetValue(c.Name.ToLower(), out var count) ? count : 0)).ToList();

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

            var is2Fa = string.Equals(twoFactorVal?.Trim(), "true", StringComparison.OrdinalIgnoreCase);
            var securityLevel = is2Fa ? "High (2FA & RBAC)" : "Standard (RBAC)";

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

            var sessionTimeout = !string.IsNullOrWhiteSpace(sessionTimeoutVal) ? sessionTimeoutVal : "30 Minutes";

            return new SettingsOverviewResponse
            {
                SecurityLevel = securityLevel,
                AlertChannels = alertChannels > 0 ? alertChannels : 1,
                SessionTimeout = sessionTimeout,
                TotalSettings = totalSettings,
                TotalCategories = categoryDtos.Count,
                Settings = rawSettings.ToDtoList(),
                Categories = categoryDtos
            };
        }

        public async Task<List<SettingCategoryDto>> GetCategoriesAsync()
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

            return categories.Select(c => c.ToDto(settingCounts.TryGetValue(c.Name.ToLower(), out var count) ? count : 0)).ToList();
        }

        public async Task<SettingCategoryDto> CreateCategoryAsync(CreateSettingCategoryRequest request, string callerName)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Category name is required.");
            }

            var trimmedName = request.Name.Trim();
            var desc = string.IsNullOrWhiteSpace(request.Description) ? $"{trimmedName} configuration and parameters" : request.Description.Trim();
            var icon = string.IsNullOrWhiteSpace(request.Icon) ? "Tune" : request.Icon.Trim();
            var now = DateTime.UtcNow;

            var existingCount = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM setting_categories
                WHERE deleted_flag = 1 AND LOWER(name) = LOWER({0})
            """, trimmedName).SingleOrDefaultAsync();

            if (existingCount > 0)
            {
                throw new BadRequestException($"Category '{trimmedName}' already exists in database.");
            }

            var newId = await _context.Database.SqlQueryRaw<int>("""
                INSERT INTO setting_categories (name, description, icon, created_at, updated_at, created_by, deleted_flag)
                VALUES ({0}, {1}, {2}, {3}, {3}, {4}, 1)
                RETURNING id AS "Value"
            """, trimmedName, desc, icon, now, callerName).SingleAsync();

            return new SettingCategoryDto
            {
                Id = newId,
                Name = trimmedName,
                Description = desc,
                Icon = icon,
                CreatedAt = now,
                CreatedBy = callerName,
                DeletedFlag = 1,
                SettingsCount = 0
            };
        }

        public async Task<SettingCategoryDto?> UpdateCategoryAsync(int id, UpdateSettingCategoryRequest request)
        {
            var category = await _context.SettingCategories
                .FromSqlRaw("""
                    SELECT id, name, description, icon, created_at, updated_at, created_by, deleted_flag
                    FROM setting_categories
                    WHERE id = {0} AND deleted_flag = 1
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (category == null) return null;

            var name = !string.IsNullOrWhiteSpace(request.Name) ? request.Name.Trim() : category.Name;
            var desc = request.Description != null ? request.Description.Trim() : category.Description;
            var icon = !string.IsNullOrWhiteSpace(request.Icon) ? request.Icon.Trim() : category.Icon;

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var dupCount = await _context.Database.SqlQueryRaw<int>("""
                    SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                    FROM setting_categories
                    WHERE id <> {0} AND deleted_flag = 1 AND LOWER(name) = LOWER({1})
                """, id, name).SingleOrDefaultAsync();

                if (dupCount > 0)
                {
                    throw new BadRequestException($"Another category with name '{name}' already exists.");
                }
            }

            var now = DateTime.UtcNow;
            await _context.Database.ExecuteSqlRawAsync("""
                UPDATE setting_categories
                SET name = {0}, description = {1}, icon = {2}, updated_at = {3}
                WHERE id = {4} AND deleted_flag = 1
            """, name, desc, icon, now, id);

            var count = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM system_settings
                WHERE LOWER(category) = LOWER({0})
            """, name).SingleOrDefaultAsync();

            return new SettingCategoryDto
            {
                Id = id,
                Name = name,
                Description = desc,
                Icon = icon,
                CreatedAt = category.CreatedAt,
                CreatedBy = category.CreatedBy,
                DeletedFlag = category.DeletedFlag,
                SettingsCount = count
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var categoryName = await _context.Database.SqlQueryRaw<string>("""
                SELECT name AS "Value"
                FROM setting_categories
                WHERE id = {0} AND deleted_flag = 1
            """, id).FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(categoryName)) return false;

            var protectedCategories = new[] { "general", "security" };
            if (protectedCategories.Contains(categoryName.ToLower()))
            {
                throw new BadRequestException($"Category '{categoryName}' is a core system category and cannot be deleted.");
            }

            var now = DateTime.UtcNow;
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                UPDATE setting_categories
                SET deleted_flag = 0, updated_at = {0}
                WHERE id = {1} AND deleted_flag = 1
            """, now, id);

            return rowsAffected > 0;
        }

        public async Task<bool> UpdateSettingsBulkAsync(UpdateSettingsBulkRequest request, string callerName)
        {
            var now = DateTime.UtcNow;

            foreach (var kvp in request.Settings)
            {
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                    UPDATE system_settings
                    SET setting_value = {0}, updated_at = {1}, updated_by = {2}
                    WHERE setting_key = {3}
                """, kvp.Value, now, callerName, kvp.Key);

                if (rowsAffected == 0)
                {
                    await _context.Database.ExecuteSqlRawAsync("""
                        INSERT INTO system_settings (setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by)
                        VALUES ({0}, {1}, 'General', {2}, 'string', {3}, {3}, {4})
                    """, kvp.Key, kvp.Value, kvp.Key, now, callerName);
                }
            }

            return true;
        }

        public async Task<SystemSettingDto> CreateSettingAsync(CreateSettingRequest request, string callerName)
        {
            if (string.IsNullOrWhiteSpace(request.SettingKey))
            {
                throw new BadRequestException("Setting key is required.");
            }

            var key = request.SettingKey.Trim();
            var val = request.SettingValue.Trim();
            var cat = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim();
            var desc = request.Description.Trim();
            var dataType = string.IsNullOrWhiteSpace(request.DataType) ? "string" : request.DataType.Trim();
            var now = DateTime.UtcNow;

            var existingCount = await _context.Database.SqlQueryRaw<int>("""
                SELECT CAST(COUNT(*) AS INTEGER) AS "Value"
                FROM system_settings
                WHERE setting_key = {0}
            """, key).SingleOrDefaultAsync();

            if (existingCount > 0)
            {
                throw new BadRequestException($"Setting with key '{key}' already exists.");
            }

            var newId = await _context.Database.SqlQueryRaw<int>("""
                INSERT INTO system_settings (setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by)
                VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {5}, {6})
                RETURNING id AS "Value"
            """, key, val, cat, desc, dataType, now, callerName).SingleAsync();

            var setting = await _context.SystemSettings
                .FromSqlRaw("""
                    SELECT id, setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by
                    FROM system_settings
                    WHERE id = {0}
                """, newId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return setting!.ToDto();
        }

        public async Task<SystemSettingDto?> UpdateSettingAsync(int id, UpdateSettingRequest request, string callerName)
        {
            var existing = await _context.SystemSettings
                .FromSqlRaw("""
                    SELECT id, setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by
                    FROM system_settings
                    WHERE id = {0}
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (existing == null) return null;

            var key = string.IsNullOrWhiteSpace(request.SettingKey) ? existing.SettingKey : request.SettingKey.Trim();
            var val = request.SettingValue ?? string.Empty;
            var cat = string.IsNullOrWhiteSpace(request.Category) ? existing.Category : request.Category.Trim();
            var desc = request.Description ?? existing.Description;
            var dataType = string.IsNullOrWhiteSpace(request.DataType) ? existing.DataType : request.DataType.Trim();
            var now = DateTime.UtcNow;

            await _context.Database.ExecuteSqlRawAsync("""
                UPDATE system_settings
                SET setting_key = {0}, setting_value = {1}, category = {2}, description = {3}, data_type = {4}, updated_at = {5}, updated_by = {6}
                WHERE id = {7}
            """, key, val, cat, desc, dataType, now, callerName, id);

            var updated = await _context.SystemSettings
                .FromSqlRaw("""
                    SELECT id, setting_key, setting_value, category, description, data_type, created_at, updated_at, updated_by
                    FROM system_settings
                    WHERE id = {0}
                """, id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return updated?.ToDto();
        }

        public async Task<bool> DeleteSettingAsync(int id)
        {
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync("""
                DELETE FROM system_settings
                WHERE id = {0}
            """, id);

            return rowsAffected > 0;
        }
    }
}
