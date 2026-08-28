using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyBackend.Application.Common.Interfaces;
using MyBackend.Application.Common.Validators;
using MyBackend.Application.Contracts;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    /// <summary>
    /// Implements user account provisioning, profile retrieval, updates, soft deletion, and permission verification using repositories and business objects.
    /// </summary>
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
            var rolesDict = await _unitOfWork.Roles.GetRoleNameDictionaryAsync();
            var designationsDict = await _unitOfWork.Designations.GetDesignationNameDictionaryAsync();

            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone ?? string.Empty,
                Age = u.Age,
                Address = u.Address ?? string.Empty,
                RoleId = u.RoleId,
                RoleName = u.RoleId.HasValue && rolesDict.TryGetValue(u.RoleId.Value, out var rName) ? rName : null,
                DesignationId = u.DesignationId,
                DesignationName = u.DesignationId.HasValue && designationsDict.TryGetValue(u.DesignationId.Value, out var dName) ? dName : null,
                DeletedFlag = u.DeletedFlag,
                IsFirstLogin = u.IsFirstLogin
            }).ToList();
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(id);
            if (user is null) return null;

            string? roleName = null;
            if (user.RoleId.HasValue)
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId.Value);
                roleName = role?.Name;
            }

            string? designationName = null;
            if (user.DesignationId.HasValue)
            {
                var designation = await _unitOfWork.Designations.GetByIdAsync(user.DesignationId.Value);
                designationName = designation?.Name;
            }

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone ?? string.Empty,
                Age = user.Age,
                Address = user.Address ?? string.Empty,
                RoleId = user.RoleId,
                RoleName = roleName,
                DesignationId = user.DesignationId,
                DesignationName = designationName,
                DeletedFlag = user.DeletedFlag,
                IsFirstLogin = user.IsFirstLogin
            };
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
                var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId.Value);
                roleName = role?.Name;
            }

            string? designationName = null;
            if (user.DesignationId.HasValue)
            {
                var designation = await _unitOfWork.Designations.GetByIdAsync(user.DesignationId.Value);
                designationName = designation?.Name;
            }

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone ?? string.Empty,
                Age = user.Age,
                Address = user.Address ?? string.Empty,
                RoleId = user.RoleId,
                RoleName = roleName,
                DesignationId = user.DesignationId,
                DesignationName = designationName,
                DeletedFlag = user.DeletedFlag,
                IsFirstLogin = user.IsFirstLogin
            };
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
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
                var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId.Value);
                roleName = role?.Name;
            }

            string? designationName = null;
            if (user.DesignationId.HasValue)
            {
                var designation = await _unitOfWork.Designations.GetByIdAsync(user.DesignationId.Value);
                designationName = designation?.Name;
            }

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone ?? string.Empty,
                Age = user.Age,
                Address = user.Address ?? string.Empty,
                RoleId = user.RoleId,
                RoleName = roleName,
                DesignationId = user.DesignationId,
                DesignationName = designationName,
                DeletedFlag = user.DeletedFlag,
                IsFirstLogin = user.IsFirstLogin
            };
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
