using System.Collections.Generic;
using System.Threading.Tasks;
using MyBackend.Application.Common.DTO;

namespace MyBackend.Application.Interfaces
{
    public interface ISettingService
    {
        Task<SettingsOverviewResponse> GetSettingsAsync(string? category, string? search);
        Task<List<SettingCategoryDto>> GetCategoriesAsync();
        Task<SettingCategoryDto> CreateCategoryAsync(CreateSettingCategoryRequest request, string callerName);
        Task<SettingCategoryDto?> UpdateCategoryAsync(int id, UpdateSettingCategoryRequest request);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> UpdateSettingsBulkAsync(UpdateSettingsBulkRequest request, string callerName);
        Task<SystemSettingDto> CreateSettingAsync(CreateSettingRequest request, string callerName);
        Task<SystemSettingDto?> UpdateSettingAsync(int id, UpdateSettingRequest request, string callerName);
        Task<bool> DeleteSettingAsync(int id);
    }
}
