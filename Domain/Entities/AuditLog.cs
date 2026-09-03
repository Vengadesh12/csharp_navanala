using System;

namespace MyBackend.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string IpAddress { get; set; } = "127.0.0.1";
        public string Status { get; set; } = "Success";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public int DeletedFlag { get; set; } = 1;

        #region Business Object Domain Methods

        public static AuditLog CreateLog(
            string action,
            string module,
            string performedBy,
            string details,
            string? ipAddress = null,
            string? status = "Success")
        {
            var now = DateTime.UtcNow;
            return new AuditLog
            {
                Action = action.Trim(),
                Module = module.Trim(),
                PerformedBy = performedBy.Trim(),
                Details = details.Trim(),
                IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? "127.0.0.1" : ipAddress.Trim(),
                Status = string.IsNullOrWhiteSpace(status) ? "Success" : status.Trim(),
                CreatedAt = now,
                UpdatedAt = now,
                DeletedFlag = 1
            };
        }

        public void SoftDelete()
        {
            DeletedFlag = 0;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}
