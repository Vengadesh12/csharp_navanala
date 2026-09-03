using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyBackend.Application.Common.Validators;
using MyBackend.Application.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Application.Mappings;
using MyBackend.Domain.Entities;
using MyBackend.Domain.Interfaces;

namespace MyBackend.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<UserService> _logger;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserService(
            IUnitOfWork unitOfWork,
            IApplicationDbContext context,
            IEmailService emailService,
            ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                    FROM users
                    ORDER BY "Id"
                """)
                .AsNoTracking()
                .ToListAsync();

            var rolesDict = await _context.Roles
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM roles
                    WHERE "DeletedFlag" = 1
                """)
                .AsNoTracking()
                .ToDictionaryAsync(r => r.Id, r => r.Name);

            var designationsDict = await _context.Designations
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Description", "DepartmentId", "DeletedFlag", "CreatedAt", "UpdatedAt"
                    FROM designations
                    WHERE "DeletedFlag" = 1
                """)
                .AsNoTracking()
                .ToDictionaryAsync(d => d.Id, d => d.Name);

            return users.ToDtoList(rolesDict, designationsDict);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                    FROM users
                    WHERE "Id" = {0}
                """, id)
                .AsNoTracking()
                .SingleOrDefaultAsync();

            if (user is null) return null;

            string? roleName = null;
            if (user.RoleId.HasValue)
            {
                roleName = await _context.Database.SqlQueryRaw<string>("""
                    SELECT "Name" AS "Value"
                    FROM roles
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, user.RoleId.Value).FirstOrDefaultAsync();
            }

            string? designationName = null;
            if (user.DesignationId.HasValue)
            {
                designationName = await _context.Database.SqlQueryRaw<string>("""
                    SELECT "Name" AS "Value"
                    FROM designations
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, user.DesignationId.Value).FirstOrDefaultAsync();
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
                roleName = await _context.Database.SqlQueryRaw<string>("""
                    SELECT "Name" AS "Value"
                    FROM roles
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, user.RoleId.Value).FirstOrDefaultAsync();
            }

            string? designationName = null;
            if (user.DesignationId.HasValue)
            {
                designationName = await _context.Database.SqlQueryRaw<string>("""
                    SELECT "Name" AS "Value"
                    FROM designations
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, user.DesignationId.Value).FirstOrDefaultAsync();
            }

            return user.ToDto(roleName, designationName);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _context.Users
                .FromSqlRaw("""
                    SELECT "Id", "Name", "Email", "Password", "Phone", "Age", "Address", "RoleId", "DesignationId", "ProfileImage", COALESCE("DeletedFlag", 1) AS "DeletedFlag", COALESCE("IsFirstLogin", false) AS "IsFirstLogin", "CreatedAt", "UpdatedAt"
                    FROM users
                    WHERE "Id" = {0}
                """, id)
                .FirstOrDefaultAsync();

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
                roleName = await _context.Database.SqlQueryRaw<string>("""
                    SELECT "Name" AS "Value"
                    FROM roles
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, user.RoleId.Value).FirstOrDefaultAsync();
            }

            string? designationName = null;
            if (user.DesignationId.HasValue)
            {
                designationName = await _context.Database.SqlQueryRaw<string>("""
                    SELECT "Name" AS "Value"
                    FROM designations
                    WHERE "Id" = {0} AND "DeletedFlag" = 1
                """, user.DesignationId.Value).FirstOrDefaultAsync();
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
