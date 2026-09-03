using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MyBackend.Application.Common.Exceptions;
using MyBackend.Application.Common.Validators;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Interfaces;
using MyBackend.Domain.Entities;

namespace MyBackend.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public ProfileService(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        public async Task<UserProfileResponse> GetProfileAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null || user.DeletedFlag != 1)
            {
                throw new NotFoundException("User profile not found or account is deactivated.");
            }

            string roleName = "Member";
            if (user.RoleId.HasValue)
            {
                roleName = await _unitOfWork.Users.GetRoleNameByIdAsync(user.RoleId.Value) ?? "Member";
            }

            var roleId = user.RoleId ?? 0;
            var designationId = user.DesignationId ?? 0;

            var permissions = await _unitOfWork.Users.GetUserPermissionKeysForProfileAsync(roleId, designationId);

            return new UserProfileResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                ProfileImage = user.ProfileImage,
                Phone = user.Phone,
                Age = user.Age,
                Address = user.Address,
                RoleId = user.RoleId,
                RoleName = roleName,
                Permissions = permissions,
                IsFirstLogin = user.IsFirstLogin
            };
        }

        public async Task<UserProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null || user.DeletedFlag != 1)
            {
                throw new NotFoundException("User profile not found.");
            }

            user.UpdateDetails(
                name: !string.IsNullOrWhiteSpace(request.Name) ? request.Name : user.Name,
                email: !string.IsNullOrWhiteSpace(request.Email) ? request.Email : user.Email,
                phone: request.Phone ?? user.Phone,
                age: request.Age > 0 ? request.Age : user.Age,
                address: request.Address ?? user.Address,
                roleId: user.RoleId,
                designationId: user.DesignationId
            );

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return await GetProfileAsync(userId);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                throw new BadRequestException("Current password is required.");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                throw new BadRequestException("New password is required.");
            }

            var (isValid, errors) = PasswordValidator.Validate(request.NewPassword);
            if (!isValid)
            {
                throw new BadRequestException(errors.Count > 0 ? errors[0] : "New password does not meet strong security requirements.");
            }

            if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
            {
                throw new BadRequestException("New password and confirm password do not match.");
            }

            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null || user.DeletedFlag != 1)
            {
                throw new NotFoundException("User profile not found.");
            }

            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
                if (verify == PasswordVerificationResult.Failed && !string.Equals(user.PasswordHash, request.CurrentPassword, StringComparison.Ordinal))
                {
                    throw new BadRequestException("Current password entered is incorrect.");
                }
            }

            var newHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.SetPasswordHash(newHash);
            user.CompleteFirstLogin();

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<UserProfileResponse> UploadProfileImageAsync(int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("No image file was provided for upload.");
            }

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".webp", ".gif", ".avif"
            };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
            {
                throw new BadRequestException("Invalid file format. Only images (.jpg, .jpeg, .png, .webp, .gif, .avif) are allowed.");
            }

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("The uploaded file does not appear to be a valid image.");
            }

            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                throw new BadRequestException("Image file size cannot exceed 5MB.");
            }

            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null || user.DeletedFlag != 1)
            {
                throw new NotFoundException("User profile not found.");
            }

            if (!string.IsNullOrWhiteSpace(user.ProfileImage))
            {
                await _fileService.DeleteFileAsync(user.ProfileImage);
            }

            await using var stream = file.OpenReadStream();
            var relativePath = await _fileService.SaveProfileImageAsync(stream, file.FileName, userId);

            user.UpdateProfileImage(relativePath);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return await GetProfileAsync(userId);
        }

        public async Task<UserProfileResponse> RemoveProfileImageAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetUserByIdAsync(userId);
            if (user == null || user.DeletedFlag != 1)
            {
                throw new NotFoundException("User profile not found.");
            }

            if (!string.IsNullOrWhiteSpace(user.ProfileImage))
            {
                await _fileService.DeleteFileAsync(user.ProfileImage);
                user.UpdateProfileImage(null);
                _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();
            }

            return await GetProfileAsync(userId);
        }
    }
}
