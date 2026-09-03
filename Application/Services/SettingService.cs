using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class SettingService : ISettingService
    {
        private readonly ISettingRepository _settingRepository;

        public SettingService(ISettingRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public async Task<SettingsOverviewResponse> GetSettingsAsync(string? category, string? search)
        {
            var (rawSettings, categoriesList, settingCounts, totalSettings, twoFactorVal, alertChannels, sessionTimeoutVal) =
                await _settingRepository.GetSettingsOverviewDataAsync(category, search);

            var categoryDtos = categoriesList
                .Select(c => c.ToDto(settingCounts.TryGetValue(c.Name.ToLower(), out var count) ? count : 0))
                .ToList();

            var is2Fa = string.Equals(twoFactorVal?.Trim(), "true", StringComparison.OrdinalIgnoreCase);
            var securityLevel = is2Fa ? "High (2FA & RBAC)" : "Standard (RBAC)";
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
            var (categories, settingCounts) = await _settingRepository.GetCategoriesWithCountsAsync();

            return categories
                .Select(c => c.ToDto(settingCounts.TryGetValue(c.Name.ToLower(), out var count) ? count : 0))
                .ToList();
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

            var exists = await _settingRepository.CategoryExistsByNameAsync(trimmedName);
            if (exists)
            {
                throw new BadRequestException($"Category '{trimmedName}' already exists in database.");
            }

            var newId = await _settingRepository.CreateCategoryAsync(trimmedName, desc, icon, callerName);

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
            var category = await _settingRepository.GetCategoryByIdAsync(id);
            if (category == null) return null;

            var name = !string.IsNullOrWhiteSpace(request.Name) ? request.Name.Trim() : category.Name;
            var desc = request.Description != null ? request.Description.Trim() : category.Description;
            var icon = !string.IsNullOrWhiteSpace(request.Icon) ? request.Icon.Trim() : category.Icon;

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var dupExists = await _settingRepository.CategoryExistsByNameAsync(name, id);
                if (dupExists)
                {
                    throw new BadRequestException($"Another category with name '{name}' already exists.");
                }
            }

            await _settingRepository.UpdateCategoryAsync(id, name, desc, icon);
            var count = await _settingRepository.GetCategorySettingCountAsync(name);

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
            var category = await _settingRepository.GetCategoryByIdAsync(id);
            if (category == null) return false;

            var protectedCategories = new[] { "general", "security" };
            if (protectedCategories.Contains(category.Name.ToLower()))
            {
                throw new BadRequestException($"Category '{category.Name}' is a core system category and cannot be deleted.");
            }

            return await _settingRepository.SoftDeleteCategoryAsync(id);
        }

        public async Task<bool> UpdateSettingsBulkAsync(UpdateSettingsBulkRequest request, string callerName)
        {
            return await _settingRepository.BulkUpdateSettingsAsync(request.Settings, callerName);
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

            var exists = await _settingRepository.SettingExistsByKeyAsync(key);
            if (exists)
            {
                throw new BadRequestException($"Setting with key '{key}' already exists.");
            }

            var newId = await _settingRepository.CreateSettingAsync(key, val, cat, desc, dataType, callerName);
            var setting = await _settingRepository.GetSettingByIdAsync(newId);

            return setting!.ToDto();
        }

        public async Task<SystemSettingDto?> UpdateSettingAsync(int id, UpdateSettingRequest request, string callerName)
        {
            var existing = await _settingRepository.GetSettingByIdAsync(id);
            if (existing == null) return null;

            var key = string.IsNullOrWhiteSpace(request.SettingKey) ? existing.SettingKey : request.SettingKey.Trim();
            var val = request.SettingValue ?? string.Empty;
            var cat = string.IsNullOrWhiteSpace(request.Category) ? existing.Category : request.Category.Trim();
            var desc = request.Description ?? existing.Description;
            var dataType = string.IsNullOrWhiteSpace(request.DataType) ? existing.DataType : request.DataType.Trim();

            await _settingRepository.UpdateSettingAsync(id, key, val, cat, desc, dataType, callerName);
            var updated = await _settingRepository.GetSettingByIdAsync(id);

            return updated?.ToDto();
        }

        public async Task<bool> DeleteSettingAsync(int id)
        {
            return await _settingRepository.DeleteSettingAsync(id);
        }
    }
}
