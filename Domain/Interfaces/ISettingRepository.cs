using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Domain.Entities;

namespace MyBackend.Domain.Interfaces
{
    public interface ISettingRepository
    {
        Task<(List<SystemSetting> Settings, List<SettingCategory> Categories, Dictionary<string, int> SettingCounts, int TotalSettings, string? TwoFactorValue, int AlertChannels, string? SessionTimeout)> GetSettingsOverviewDataAsync(string? category, string? search);

        Task<(List<SettingCategory> Categories, Dictionary<string, int> SettingCounts)> GetCategoriesWithCountsAsync();

        Task<bool> CategoryExistsByNameAsync(string name, int? excludeId = null);

        Task<int> CreateCategoryAsync(string name, string description, string icon, string createdBy);

        Task<SettingCategory?> GetCategoryByIdAsync(int id);

        Task<int> GetCategorySettingCountAsync(string categoryName);

        Task<bool> UpdateCategoryAsync(int id, string name, string description, string icon);

        Task<bool> SoftDeleteCategoryAsync(int id);

        Task<bool> BulkUpdateSettingsAsync(IDictionary<string, string> settings, string updatedBy);

        Task<bool> SettingExistsByKeyAsync(string key);

        Task<int> CreateSettingAsync(string key, string value, string category, string description, string dataType, string createdBy);

        Task<SystemSetting?> GetSettingByIdAsync(int id);

        Task<bool> UpdateSettingAsync(int id, string key, string value, string category, string description, string dataType, string updatedBy);

        Task<bool> DeleteSettingAsync(int id);

        Task<string?> GetSettingValueAsync(string key);
    }
}
