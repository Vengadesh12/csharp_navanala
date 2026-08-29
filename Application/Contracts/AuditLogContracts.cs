using System;
using System.Collections.Generic;

namespace MyBackend.Application.Contracts
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Status { get; set; } = "Success";
        public DateTime CreatedAt { get; set; }
        public int DeletedFlag { get; set; } = 1;
    }

    public class CreateAuditLogRequest
    {
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = "System";
        public string Details { get; set; } = string.Empty;
        public string Status { get; set; } = "Success";
        public string? IpAddress { get; set; }
    }

    public class AuditLogOverviewResponse
    {
        public int TotalEvents { get; set; }
        public int SuccessfulLogins { get; set; }
        public int PrivilegeChanges { get; set; }
        public List<AuditLogDto> Logs { get; set; } = [];
    }
}
