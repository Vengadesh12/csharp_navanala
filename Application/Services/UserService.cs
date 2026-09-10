using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyBackend.Application.Common.Validators;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Models;

namespace MyBackend.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IPermissionHierarchyService _permissionHierarchyService;
        private readonly ILogger<UserService> _logger;
        private readonly PasswordHasher<UserModel> _passwordHasher = new();

        public UserService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IPermissionHierarchyService permissionHierarchyService,
            ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _permissionHierarchyService = permissionHierarchyService;
            _logger = logger;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllUsersAsync();
            var rolesDict = await _unitOfWork.Users.GetActiveRolesLookupAsync();
            var designationsDict = await _unitOfWork.Users.GetActiveDesignationsLookupAsync();

            return users.ToDtoList(rolesDict, designationsDict);
        }

        public async Task<PagedResult<object>> GetUsersPagedAsync(DynamicQueryParameters query)
        {
            var allUsers = await _unitOfWork.Users.GetAllUsersAsync();
            var rolesDict = await _unitOfWork.Users.GetActiveRolesLookupAsync();
            var designationsDict = await _unitOfWork.Users.GetActiveDesignationsLookupAsync();

            var dtoList = allUsers.ToDtoList(rolesDict, designationsDict).AsQueryable();

            // Status filter
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                if (query.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    dtoList = dtoList.Where(u => u.DeletedFlag == 1);
                }
                else if (query.Status.Equals("inactive", StringComparison.OrdinalIgnoreCase) ||
                         query.Status.Equals("deleted", StringComparison.OrdinalIgnoreCase))
                {
                    dtoList = dtoList.Where(u => u.DeletedFlag == 0);
                }
            }

            // Multiple composable filters: search across name, email, phone, role
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var s = query.Search.Trim().ToLowerInvariant();
                dtoList = dtoList.Where(u =>
                    (u.Name != null && u.Name.ToLower().Contains(s)) ||
                    (u.Email != null && u.Email.ToLower().Contains(s)) ||
                    (u.Phone != null && u.Phone.ToLower().Contains(s)) ||
                    (u.RoleName != null && u.RoleName.ToLower().Contains(s)));
            }

            // Role / Category filter
            if (!string.IsNullOrWhiteSpace(query.Category))
            {
                var cat = query.Category.Trim().ToLowerInvariant();
                dtoList = dtoList.Where(u => u.RoleName != null && u.RoleName.ToLower().Contains(cat));
            }

            // Department filter
            if (!string.IsNullOrWhiteSpace(query.Department))
            {
                var dept = query.Department.Trim().ToLowerInvariant();
                dtoList = dtoList.Where(u => u.DesignationName != null && u.DesignationName.ToLower().Contains(dept));
            }

            // Range filter: Age
            if (query.MinAge.HasValue)
            {
                dtoList = dtoList.Where(u => u.Age >= query.MinAge.Value);
            }
            if (query.MaxAge.HasValue)
            {
                dtoList = dtoList.Where(u => u.Age <= query.MaxAge.Value);
            }

            var totalCount = dtoList.Count();

            // Dynamic Sorting
            dtoList = MyBackend.Application.Common.Extensions.QueryableExtensions.ApplySorting(
                dtoList, query.SortBy, query.SortOrder, "Id");

            // Dynamic Pagination
            var pageItems = MyBackend.Application.Common.Extensions.QueryableExtensions.ApplyPagination(
                dtoList, query.Page, query.PageSize).ToList();

            // Dynamic Field Selection with sensitive field blocklist
            var shaped = MyBackend.Application.Common.Helpers.FieldSelector.ShapeData<UserDto>(
                pageItems, query.Fields, query.Include, query.Exclude).ToList();

            return new PagedResult<object>
            {
                Success = true,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                Data = shaped
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user is null) return null;

            string? roleName = null;
            if (user.RoleId.HasValue)
            {
                roleName = await _unitOfWork.Users.GetRoleNameByIdAsync(user.RoleId.Value);
            }

            string? designationName = null;
            if (user.DesignationId.HasValue)
            {
                designationName = await _unitOfWork.Users.GetDesignationNameByIdAsync(user.DesignationId.Value);
            }

            return user.ToDto(roleName, designationName);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            var plainPassword = request.Password;

            var (isValid, errors) = PasswordValidator.Validate(plainPassword);
            if (!isValid)
            {
                throw new ArgumentException(errors.Count > 0 ? errors[0] : "Password does not meet strong security requirements.");
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            if (await _unitOfWork.Users.EmailExistsAsync(normalizedEmail))
            {
                throw new InvalidOperationException($"A user with email '{request.Email.Trim()}' already exists.");
            }

            // Create User entity directly
            var now = DateTime.UtcNow;
            var user = new UserModel
            {
                Name = request.Name.Trim(),
                Email = normalizedEmail,
                Phone = request.Phone?.Trim() ?? string.Empty,
                Age = request.Age,
                Address = request.Address?.Trim() ?? string.Empty,
                RoleId = request.RoleId,
                DesignationId = request.DesignationId,
                DeletedFlag = 1,
                IsFirstLogin = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            var hashedPassword = _passwordHasher.HashPassword(user, plainPassword);
            user.PasswordHash = hashedPassword;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            // Dispatch welcome credentials email via Gmail SMTP
            try
            {
                await _emailService.SendWelcomeUserEmailAsync(user.Email, user.Name, plainPassword);
                _logger.LogInformation("Welcome credentials email dispatched successfully to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome credentials email to {Email}", user.Email);
            }

            string? roleName = null;
            if (user.RoleId.HasValue)
            {
                roleName = await _unitOfWork.Users.GetRoleNameByIdAsync(user.RoleId.Value);
            }

            string? designationName = null;
            if (user.DesignationId.HasValue)
            {
                designationName = await _unitOfWork.Users.GetDesignationNameByIdAsync(user.DesignationId.Value);
            }

            return user.ToDto(roleName, designationName);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user is null) return null;

            // Update user properties directly
            if (!string.IsNullOrWhiteSpace(request.Name)) user.Name = request.Name.Trim();
            if (!string.IsNullOrWhiteSpace(request.Email)) user.Email = request.Email.Trim().ToLowerInvariant();
            if (request.Phone != null) user.Phone = request.Phone.Trim();
            if (request.Age > 0) user.Age = request.Age;
            if (request.Address != null) user.Address = request.Address.Trim();
            user.RoleId = request.RoleId;
            user.DesignationId = request.DesignationId;
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var (isValid, errors) = PasswordValidator.Validate(request.Password);
                if (!isValid)
                {
                    throw new ArgumentException(errors.Count > 0 ? errors[0] : "Password does not meet strong security requirements.");
                }

                var newHash = _passwordHasher.HashPassword(user, request.Password);
                user.PasswordHash = newHash;
                user.UpdatedAt = DateTime.UtcNow;
            }

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            string? roleName = null;
            if (user.RoleId.HasValue)
            {
                roleName = await _unitOfWork.Users.GetRoleNameByIdAsync(user.RoleId.Value);
            }

            string? designationName = null;
            if (user.DesignationId.HasValue)
            {
                designationName = await _unitOfWork.Users.GetDesignationNameByIdAsync(user.DesignationId.Value);
            }

            return user.ToDto(roleName, designationName);
        }

        public async Task<bool> SoftDeleteUserAsync(int id)
        {
            return await _unitOfWork.Users.SetDeletedFlagAsync(id, 0);
        }

        public async Task<bool> RestoreUserAsync(int id)
        {
            return await _unitOfWork.Users.SetDeletedFlagAsync(id, 1);
        }

        public async Task<bool> HasPermissionAsync(int userId, params string[] permissionKeys)
        {
            if (permissionKeys.Length == 0) return true;

            foreach (var key in permissionKeys)
            {
                if (await _permissionHierarchyService.HasPermissionAsync(userId, key))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
