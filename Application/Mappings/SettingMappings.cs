using System.Collections.Generic;
using System.Linq;
using MyBackend.Application.Contracts;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Mappings
{
    public static class SettingMappings
    {
        public static SystemSettingDto ToDto(this SystemSetting entity)
        {
            return new SystemSettingDto
            {
                Id = entity.Id,
                SettingKey = entity.SettingKey,
                SettingValue = entity.SettingValue,
                Category = entity.Category,
                Description = entity.Description,
                DataType = entity.DataType,
                UpdatedBy = entity.UpdatedBy,
                UpdatedAt = entity.UpdatedAt,
                DeletedFlag = 1
            };
        }

        public static List<SystemSettingDto> ToDtoList(this IEnumerable<SystemSetting> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }

        public static SettingCategoryDto ToDto(this SettingCategory entity, int settingsCount = 0)
        {
            return new SettingCategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description ?? string.Empty,
                Icon = entity.Icon ?? "Tune",
                CreatedAt = entity.CreatedAt,
                CreatedBy = entity.CreatedBy ?? "System Admin",
                DeletedFlag = entity.DeletedFlag,
                SettingsCount = settingsCount
            };
        }

        public static List<SettingCategoryDto> ToDtoList(this IEnumerable<SettingCategory> entities, Dictionary<string, int>? categoryCounts = null)
        {
            return entities.Select(e =>
            {
                var count = categoryCounts != null && categoryCounts.TryGetValue(e.Name, out var c) ? c : 0;
                return e.ToDto(count);
            }).ToList();
        }
    }
}
