using System;
using System.Collections.Generic;
using MyBackend.Application.Common.Exceptions;

namespace MyBackend.Application.Common.Validators
{
    public static class BusinessRuleValidator
    {
        public static void ValidatePasswordNotEmail(string? password, string? email)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email)) return;

            if (string.Equals(password.Trim(), email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var errors = new Dictionary<string, string[]>
                {
                    ["password"] = new[] { "Password must not be identical to email address." }
                };
                throw new ValidationException(errors);
            }
        }

        public static void ValidateConfirmPassword(string? password, string? confirmPassword)
        {
            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                var errors = new Dictionary<string, string[]>
                {
                    ["confirmPassword"] = new[] { "Confirm password does not match password." }
                };
                throw new ValidationException(errors);
            }
        }

        public static void ValidateRange<T>(T? min, T? max, string minFieldName, string maxFieldName) where T : struct, IComparable<T>
        {
            if (min.HasValue && max.HasValue && min.Value.CompareTo(max.Value) > 0)
            {
                var errors = new Dictionary<string, string[]>
                {
                    [minFieldName] = new[] { $"{minFieldName} cannot be greater than {maxFieldName}." }
                };
                throw new ValidationException(errors);
            }
        }

        public static void ValidateDateRange(DateTime? start, DateTime? end, string startFieldName = "startDate", string endFieldName = "endDate")
        {
            if (start.HasValue && end.HasValue && start.Value > end.Value)
            {
                var errors = new Dictionary<string, string[]>
                {
                    [startFieldName] = new[] { $"{startFieldName} cannot be later than {endFieldName}." }
                };
                throw new ValidationException(errors);
            }
        }

        public static void ValidateManagerDepartmentRequirement(int? roleId, string? roleName, int? departmentId)
        {
            var isManager = roleId == 3 ||
                (roleName != null && roleName.Contains("manager", StringComparison.OrdinalIgnoreCase));

            if (isManager && (!departmentId.HasValue || departmentId.Value <= 0))
            {
                var errors = new Dictionary<string, string[]>
                {
                    ["departmentId"] = new[] { "Department is required when role is Manager." }
                };
                throw new ValidationException(errors);
            }
        }

        public static void ValidateConditionalRequired(bool condition, string? value, string fieldName, string reason)
        {
            if (condition && string.IsNullOrWhiteSpace(value))
            {
                var errors = new Dictionary<string, string[]>
                {
                    [fieldName] = new[] { $"{fieldName} is required because {reason}." }
                };
                throw new ValidationException(errors);
            }
        }

        public static void ValidateStatusTransition(
            string currentStatus,
            string newStatus,
            IDictionary<string, HashSet<string>> allowedTransitions,
            string entityName)
        {
            if (string.IsNullOrWhiteSpace(currentStatus) || string.IsNullOrWhiteSpace(newStatus)) return;
            if (string.Equals(currentStatus, newStatus, StringComparison.OrdinalIgnoreCase)) return;

            if (allowedTransitions.TryGetValue(currentStatus.ToLowerInvariant(), out var targets))
            {
                if (!targets.Contains(newStatus.ToLowerInvariant()))
                {
                    var errors = new Dictionary<string, string[]>
                    {
                        ["status"] = new[] { $"Invalid status transition for {entityName} from '{currentStatus}' to '{newStatus}'." }
                    };
                    throw new ValidationException(errors);
                }
            }
        }
    }
}
