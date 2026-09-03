using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MyBackend.Domain.Entities.Model
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
    }
}
