using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace MyBackend.Application.Common.Helpers
{
    public static class FieldSelector
    {
        // Global blocklist of sensitive property names that MUST NEVER be exposed
        private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "Password",
            "PasswordHash",
            "SecurityKey",
            "Token",
            "RefreshToken",
            "Secret",
            "AppPassword",
            "SmtpPassword",
            "DatabasePassword",
            "InternalCredentials",
            "PrivateKey"
        };

        public static object ShapeData<T>(
            T source,
            string? fields,
            string? include = null,
            string? exclude = null)
        {
            if (source == null) return new ExpandoObject();

            // If no field selection is specified, return original object for complete backward compatibility
            if (string.IsNullOrWhiteSpace(fields) &&
                string.IsNullOrWhiteSpace(include) &&
                string.IsNullOrWhiteSpace(exclude))
            {
                return source;
            }

            return ShapeSingleObject(source, fields, include, exclude);
        }

        public static IEnumerable<object> ShapeData<T>(
            IEnumerable<T> source,
            string? fields,
            string? include = null,
            string? exclude = null)
        {
            if (source == null) return Enumerable.Empty<object>();

            // If no field selection is specified, return original list for complete backward compatibility
            if (string.IsNullOrWhiteSpace(fields) &&
                string.IsNullOrWhiteSpace(include) &&
                string.IsNullOrWhiteSpace(exclude))
            {
                return source.Cast<object>();
            }

            var resultList = new List<object>();
            foreach (var item in source)
            {
                if (item != null)
                {
                    resultList.Add(ShapeSingleObject(item, fields, include, exclude));
                }
            }
            return resultList;
        }

        private static ExpandoObject ShapeSingleObject<T>(
            T source,
            string? fields,
            string? include,
            string? exclude)
        {
            var expando = new ExpandoObject();
            var expandoDict = (IDictionary<string, object?>)expando;

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => !SensitiveFields.Contains(p.Name))
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            // Determine included properties
            var selectedPropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var explicitFields = (fields ?? include)?
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (explicitFields != null && explicitFields.Length > 0)
            {
                foreach (var field in explicitFields)
                {
                    // Strict security check: deny sensitive fields even if requested
                    if (SensitiveFields.Contains(field)) continue;

                    if (properties.TryGetValue(field, out var prop))
                    {
                        selectedPropertyNames.Add(prop.Name);
                    }
                }
            }
            else
            {
                foreach (var prop in properties.Values)
                {
                    selectedPropertyNames.Add(prop.Name);
                }
            }

            // Exclude specified fields
            if (!string.IsNullOrWhiteSpace(exclude))
            {
                var excludedFields = exclude.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var ex in excludedFields)
                {
                    selectedPropertyNames.Remove(ex);
                }
            }

            // Populate shaped object with camelCase keys
            foreach (var propName in selectedPropertyNames)
            {
                if (properties.TryGetValue(propName, out var propInfo))
                {
                    var propValue = propInfo.GetValue(source);
                    var camelCaseName = char.ToLowerInvariant(propInfo.Name[0]) + propInfo.Name[1..];
                    expandoDict[camelCaseName] = propValue;
                }
            }

            return expando;
        }
    }
}
