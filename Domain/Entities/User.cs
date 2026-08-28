using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MyBackend.Domain.Entities
{
    /// <summary>
    /// User account business object representing a registered system member with domain behaviors.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique user identifier.
        /// </summary>
        /// <example>1</example>
        public int Id { get; set; }

        /// <summary>
        /// Full name of the user.
        /// </summary>
        /// <example>Alex Morgan</example>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email address used for authentication.
        /// </summary>
        /// <example>alex.morgan@example.com</example>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Stored cryptographic hash of the user password.
        /// </summary>
        [JsonIgnore]
        [Column("Password")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Plain-text password transient property (used during creation/update).
        /// </summary>
        [NotMapped]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the assigned role.
        /// </summary>
        /// <example>2</example>
        public int? RoleId { get; set; }

        /// <summary>
        /// Identifier of the assigned designation.
        /// </summary>
        /// <example>1</example>
        public int? DesignationId { get; set; }

        /// <summary>
        /// Contact telephone number.
        /// </summary>
        /// <example>+1 (555) 019-2834</example>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// User age.
        /// </summary>
        /// <example>32</example>
        public int Age { get; set; }

        /// <summary>
        /// Physical address.
        /// </summary>
        /// <example>123 Innovation Way, Suite 400</example>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Status flag (1 = Active, 0 = Deactivated/Deleted).
        /// </summary>
        /// <example>1</example>
        public int DeletedFlag { get; set; } = 1;

        /// <summary>
        /// Flag indicating whether the user must change password on their initial login.
        /// </summary>
        /// <example>true</example>
        public bool IsFirstLogin { get; set; } = false;

        #region Business Object Domain Methods

        /// <summary>
        /// Business Object Factory Method to create a newly provisioned User.
        /// </summary>
        public static User Create(
            string name,
            string email,
            string? phone = null,
            int age = 0,
            string? address = null,
            int? roleId = null,
            int? designationId = null,
            bool isFirstLogin = true)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("User name is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("User email is required.", nameof(email));

            return new User
            {
                Name = name.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                Phone = phone?.Trim() ?? string.Empty,
                Age = age,
                Address = address?.Trim() ?? string.Empty,
                RoleId = roleId,
                DesignationId = designationId,
                DeletedFlag = 1,
                IsFirstLogin = isFirstLogin
            };
        }

        /// <summary>
        /// Updates the user's personal profile and RBAC assignments.
        /// </summary>
        public void UpdateDetails(
            string name,
            string email,
            string? phone,
            int age,
            string? address,
            int? roleId,
            int? designationId)
        {
            if (!string.IsNullOrWhiteSpace(name)) Name = name.Trim();
            if (!string.IsNullOrWhiteSpace(email)) Email = email.Trim().ToLowerInvariant();
            if (phone != null) Phone = phone.Trim();
            if (age > 0) Age = age;
            if (address != null) Address = address.Trim();
            RoleId = roleId;
            DesignationId = designationId;
        }

        /// <summary>
        /// Updates the user's password hash and marks the first-login flag complete if applicable.
        /// </summary>
        public void SetPasswordHash(string newPasswordHash, bool clearFirstLoginFlag = false)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            if (clearFirstLoginFlag)
            {
                IsFirstLogin = false;
            }
        }

        /// <summary>
        /// Marks the initial mandatory password reset requirement complete.
        /// </summary>
        public void CompleteFirstLogin()
        {
            IsFirstLogin = false;
        }

        /// <summary>
        /// Soft-deletes / deactivates the user account.
        /// </summary>
        public void SoftDelete()
        {
            DeletedFlag = 0;
        }

        /// <summary>
        /// Restores / reactivates the user account.
        /// </summary>
        public void Restore()
        {
            DeletedFlag = 1;
        }

        /// <summary>
        /// Returns whether the user is active.
        /// </summary>
        public bool IsActiveAccount() => DeletedFlag == 1;

        #endregion
    }
}