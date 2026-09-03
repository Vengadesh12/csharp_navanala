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
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllUsersAsync();
            var rolesDict = await _unitOfWork.Users.GetActiveRolesLookupAsync();
            var designationsDict = await _unitOfWork.Users.GetActiveDesignationsLookupAsync();

            return users.ToDtoList(rolesDict, designationsDict);
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

            // Create using Business Object Factory Method
            var user = User.Create(
                name: request.Name,
                email: request.Email,
                phone: request.Phone,
                age: request.Age,
                address: request.Address,
                roleId: request.RoleId,
                designationId: request.DesignationId,
                isFirstLogin: true
            );

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            var hashedPassword = _passwordHasher.HashPassword(user, plainPassword);
            user.SetPasswordHash(hashedPassword);

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            // Dispatch welcome credentials email via Gmail SMTP
            try
            {
                await _emailService.SendWelcomeUserEmailAsync(user.Email, user.Name, plainPassword);
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

            // Use Business Object update method
            user.UpdateDetails(
                name: request.Name,
                email: request.Email,
                phone: request.Phone,
                age: request.Age,
                address: request.Address,
                roleId: request.RoleId,
                designationId: request.DesignationId
            );

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var (isValid, errors) = PasswordValidator.Validate(request.Password);
                if (!isValid)
                {
                    throw new ArgumentException(errors.Count > 0 ? errors[0] : "Password does not meet strong security requirements.");
                }

                var newHash = _passwordHasher.HashPassword(user, request.Password);
                user.SetPasswordHash(newHash);
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
            return await _unitOfWork.Users.HasPermissionAsync(userId, permissionKeys);
        }
    }
}
