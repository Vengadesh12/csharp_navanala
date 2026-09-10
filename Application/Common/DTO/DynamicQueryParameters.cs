using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.Common.DTO
{
    public class DynamicQueryParameters : IValidatableObject
    {
        private int _page = 1;
        private int _pageSize = 20;

        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 1 : (value > 100 ? 100 : value);
        }

        public string? Search { get; set; }

        public string? SortBy { get; set; }

        public string? SortOrder { get; set; } = "asc"; // "asc" or "desc"

        public string? Status { get; set; }

        public string? Category { get; set; }

        public string? Department { get; set; }

        // Range Filters
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }

        public decimal? MinSalary { get; set; }
        public decimal? MaxSalary { get; set; }

        // Field Selection
        public string? Fields { get; set; }
        public string? Include { get; set; }
        public string? Exclude { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (StartDate.HasValue && EndDate.HasValue && StartDate.Value > EndDate.Value)
            {
                yield return new ValidationResult(
                    "StartDate cannot be later than EndDate.",
                    new[] { nameof(StartDate), nameof(EndDate) }
                );
            }

            if (MinAmount.HasValue && MaxAmount.HasValue && MinAmount.Value > MaxAmount.Value)
            {
                yield return new ValidationResult(
                    "MinAmount cannot be greater than MaxAmount.",
                    new[] { nameof(MinAmount), nameof(MaxAmount) }
                );
            }

            if (MinAge.HasValue && MaxAge.HasValue && MinAge.Value > MaxAge.Value)
            {
                yield return new ValidationResult(
                    "MinAge cannot be greater than MaxAge.",
                    new[] { nameof(MinAge), nameof(MaxAge) }
                );
            }

            if (MinSalary.HasValue && MaxSalary.HasValue && MinSalary.Value > MaxSalary.Value)
            {
                yield return new ValidationResult(
                    "MinSalary cannot be greater than MaxSalary.",
                    new[] { nameof(MinSalary), nameof(MaxSalary) }
                );
            }

            if (!string.IsNullOrWhiteSpace(SortOrder))
            {
                var lowerOrder = SortOrder.Trim().ToLowerInvariant();
                if (lowerOrder != "asc" && lowerOrder != "desc")
                {
                    yield return new ValidationResult(
                        "SortOrder must be either 'asc' or 'desc'.",
                        new[] { nameof(SortOrder) }
                    );
                }
            }
        }
    }

    public class PagedResult<T>
    {
        public bool Success { get; set; } = true;
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public List<T> Data { get; set; } = [];
    }
}
