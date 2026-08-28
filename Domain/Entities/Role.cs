using System;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// Represents an authorization role business object assigned to users in the RBAC system.
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Unique role identifier.
        /// </summary>
        /// <example>2</example>
        public int Id { get; set; }

        /// <summary>
        /// Name of the role.
        /// </summary>
        /// <example>Super Admin</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the role responsibilities and privileges.
        /// </summary>
        /// <example>Full system access and authority to manage all workspaces and permissions.</example>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Status flag (1 = Active, 0 = Deleted).
        /// </summary>
        /// <example>1</example>
        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

        /// <summary>
        /// Factory method to create a new Role.
        /// </summary>
        public static Role Create(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name is required.", nameof(name));

            return new Role
            {
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty,
                DeletedFlag = 1
            };
        }

        /// <summary>
        /// Updates the role details.
        /// </summary>
        public void UpdateDetails(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name cannot be empty.", nameof(name));

            Name = name.Trim();
            Description = description?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Soft deletes the role.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
        }

        /// <summary>
        /// Restores a soft-deleted role.
        /// </summary>
        public void Restore()
        {
            DeletedFlag = 1;
        }

        #endregion
    }
}
