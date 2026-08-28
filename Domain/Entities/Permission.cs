using System;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// System permission business object defining a specific platform capability.
    /// </summary>
    public class Permission
    {
        public int Id { get; set; }
        public string PermissionKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

        /// <summary>
        /// Factory method to create a new Permission.
        /// </summary>
        public static Permission Create(string permissionKey, string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(permissionKey))
                throw new ArgumentException("Permission key is required.", nameof(permissionKey));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Permission name is required.", nameof(name));

            return new Permission
            {
                PermissionKey = permissionKey.Trim(),
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                DeletedFlag = 1
            };
        }

        /// <summary>
        /// Updates the permission display name and description.
        /// </summary>
        public void UpdateDetails(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Permission name cannot be empty.", nameof(name));

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Soft deletes the permission.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
        }

        /// <summary>
        /// Restores a soft-deleted permission.
        /// </summary>
        public void Restore()
        {
            DeletedFlag = 1;
        }

        #endregion
    }
}
