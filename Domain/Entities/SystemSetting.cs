using System;

namespace MyBackend.Domain.Entities
{
    public class SystemSetting
    {
        public int Id { get; set; }
        public string SettingKey { get; set; } = string.Empty;
        public string SettingValue { get; set; } = string.Empty;
        public string Category { get; set; } = "General";
        public string Description { get; set; } = string.Empty;
        public string DataType { get; set; } = "string";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; } = "System Admin";

        #region Business Object Domain Methods

        public static SystemSetting Create(
            string settingKey,
            string settingValue,
            string? category,
            string? description,
            string? dataType,
            string? updatedBy)
        {
            if (string.IsNullOrWhiteSpace(settingKey))
                throw new ArgumentException("Setting key is required.", nameof(settingKey));

            var now = DateTime.UtcNow;
            return new SystemSetting
            {
                SettingKey = settingKey.Trim(),
                SettingValue = settingValue ?? string.Empty,
                Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
                Description = description?.Trim() ?? string.Empty,
                DataType = string.IsNullOrWhiteSpace(dataType) ? "string" : dataType.Trim(),
                CreatedAt = now,
                UpdatedAt = now,
                UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "System Admin" : updatedBy.Trim()
            };
        }

        public void UpdateValue(
            string settingValue,
            string updatedBy,
            string? category = null,
            string? description = null,
            string? dataType = null)
        {
            SettingValue = settingValue ?? string.Empty;
            UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "System Admin" : updatedBy.Trim();
            UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(category)) Category = category.Trim();
            if (description != null) Description = description.Trim();
            if (!string.IsNullOrWhiteSpace(dataType)) DataType = dataType.Trim();
        }

        #endregion
    }
}
