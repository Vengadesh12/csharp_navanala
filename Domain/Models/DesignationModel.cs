using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBackend.Domain.Models
{
    [Table("designations")]
    public class DesignationModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }

        public int DeletedFlag { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public DepartmentModel? Department { get; set; }
    }
}
