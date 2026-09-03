using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MyBackend.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        [Column("Password")]
        public string PasswordHash { get; set; } = string.Empty;

        [NotMapped]
        public string Password { get; set; } = string.Empty;

        public int? RoleId { get; set; }

        public int? DesignationId { get; set; }

        public string Phone { get; set; } = string.Empty;

        public int Age { get; set; }

        public string Address { get; set; } = string.Empty;

        public string? ProfileImage { get; set; }

        public int DeletedFlag { get; set; } = 1;

        public bool IsFirstLogin { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        #region Business Object Domain Methods

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

            var now = DateTime.UtcNow;
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
                IsFirstLogin = isFirstLogin,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

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
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateProfileImage(string? profileImageUrl)
        {
            ProfileImage = profileImageUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPasswordHash(string newPasswordHash, bool clearFirstLoginFlag = false)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Password hash cannot be empty.", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            if (clearFirstLoginFlag)
            {
                IsFirstLogin = false;
            }
            UpdatedAt = DateTime.UtcNow;
        }

        public void CompleteFirstLogin()
        {
            IsFirstLogin = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            DeletedFlag = 1;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsActiveAccount() => DeletedFlag == 1;

        #endregion
    }
}
