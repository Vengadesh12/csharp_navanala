using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using MyBackend.Application.Common.DTO;
using MyBackend.Application.Common.Extensions;
using MyBackend.Application.Common.Helpers;
using Xunit;

namespace UnitTests.Application
{
    public class DynamicQueryTests
    {
        public class SampleItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
            public decimal Amount { get; set; }
            public string Password { get; set; } = "Secret123";
            public string Token { get; set; } = "SensitiveToken";
        }

        [Fact]
        public void ApplySorting_SortsAscendingAndDescending()
        {
            var data = new List<SampleItem>
            {
                new() { Id = 3, Name = "Charlie", Age = 35 },
                new() { Id = 1, Name = "Alice", Age = 25 },
                new() { Id = 2, Name = "Bob", Age = 30 }
            }.AsQueryable();

            // Ascending sort by Name
            var sortedAsc = QueryableExtensions.ApplySorting(data, "Name", "asc", "Id").ToList();
            Assert.Equal("Alice", sortedAsc[0].Name);
            Assert.Equal("Bob", sortedAsc[1].Name);
            Assert.Equal("Charlie", sortedAsc[2].Name);

            // Descending sort by Age
            var sortedDesc = QueryableExtensions.ApplySorting(data, "Age", "desc", "Id").ToList();
            Assert.Equal(35, sortedDesc[0].Age);
            Assert.Equal(30, sortedDesc[1].Age);
            Assert.Equal(25, sortedDesc[2].Age);
        }

        [Fact]
        public void ApplyPagination_ReturnsCorrectPage()
        {
            var data = Enumerable.Range(1, 50).Select(i => new SampleItem { Id = i, Name = $"Item {i}" }).AsQueryable();

            var page2 = QueryableExtensions.ApplyPagination(data, page: 2, pageSize: 10).ToList();

            Assert.Equal(10, page2.Count);
            Assert.Equal(11, page2[0].Id);
            Assert.Equal(20, page2[^1].Id);
        }

        [Fact]
        public void FieldSelector_ShapesData_And_RedactsSensitiveFields()
        {
            var items = new List<SampleItem>
            {
                new() { Id = 1, Name = "Alice", Age = 25, Password = "secret", Token = "tok1" },
                new() { Id = 2, Name = "Bob", Age = 30, Password = "secret", Token = "tok2" }
            };

            // Request fields including "Password" and "Token"
            var shapedList = FieldSelector.ShapeData<SampleItem>(items, fields: "id,name,password,token").OfType<ExpandoObject>().ToList();

            Assert.Equal(2, shapedList.Count);
            foreach (var expando in shapedList)
            {
                var dict = (IDictionary<string, object?>)expando;
                // Requested safe fields should be present
                Assert.True(dict.ContainsKey("id"));
                Assert.True(dict.ContainsKey("name"));
                // Sensitive fields MUST be redacted / omitted
                Assert.False(dict.ContainsKey("password"));
                Assert.False(dict.ContainsKey("Password"));
                Assert.False(dict.ContainsKey("token"));
                Assert.False(dict.ContainsKey("Token"));
            }
        }

        [Fact]
        public void RangeValidation_StartDateGreaterThanEndDate_FailsValidation()
        {
            var query = new DynamicQueryParameters
            {
                StartDate = new DateTime(2026, 12, 31),
                EndDate = new DateTime(2026, 1, 1)
            };

            var context = new ValidationContext(query);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(query, context, results, validateAllProperties: true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("StartDate cannot be later than EndDate"));
        }

        [Fact]
        public void RangeValidation_MinAgeGreaterThanMaxAge_FailsValidation()
        {
            var query = new DynamicQueryParameters
            {
                MinAge = 50,
                MaxAge = 20
            };

            var context = new ValidationContext(query);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(query, context, results, validateAllProperties: true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("MinAge cannot be greater than MaxAge"));
        }

        [Fact]
        public void RangeValidation_MinAmountGreaterThanMaxAmount_FailsValidation()
        {
            var query = new DynamicQueryParameters
            {
                MinAmount = 1000m,
                MaxAmount = 50m
            };

            var context = new ValidationContext(query);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(query, context, results, validateAllProperties: true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("MinAmount cannot be greater than MaxAmount"));
        }

        [Fact]
        public void RangeValidation_ValidRanges_PassValidation()
        {
            var query = new DynamicQueryParameters
            {
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                MinAge = 18,
                MaxAge = 65,
                MinAmount = 10m,
                MaxAmount = 500m
            };

            var context = new ValidationContext(query);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(query, context, results, validateAllProperties: true);

            Assert.True(isValid);
            Assert.Empty(results);
        }
    }
}
