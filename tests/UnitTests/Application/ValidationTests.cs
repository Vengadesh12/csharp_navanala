using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MyBackend.Application.Common.DTO;
using Xunit;

namespace UnitTests.Application
{
    public class ValidationTests
    {
        [Fact]
        public void CreateUserRequest_CrossFieldValidation_Fails_WhenPasswordEqualsEmail()
        {
            var request = new CreateUserRequest
            {
                Email = "john.doe@example.com",
                Name = "John Doe",
                Password = "john.doe@example.com", // Password equals email
                RoleId = 3,
                DesignationId = 1,
                Age = 28
            };

            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("Password cannot be the same as email"));
        }

        [Fact]
        public void CreateUserRequest_Fails_WhenAgeOutOfRange()
        {
            var request = new CreateUserRequest
            {
                Email = "john.doe@example.com",
                Name = "John Doe",
                Password = "StrongPassword123!",
                RoleId = 3,
                DesignationId = 1,
                Age = 150 // Invalid age
            };

            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage != null && r.ErrorMessage.Contains("Age must be between 18 and 120"));
        }

        [Fact]
        public void CreateUserRequest_Fails_WhenEmailInvalid()
        {
            var request = new CreateUserRequest
            {
                Email = "not-an-email",
                Name = "John Doe",
                Password = "StrongPassword123!",
                RoleId = 3,
                DesignationId = 1,
                Age = 25
            };

            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

            Assert.False(isValid);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void CreateUserRequest_ValidModel_PassesValidation()
        {
            var request = new CreateUserRequest
            {
                Email = "alice@example.com",
                Name = "Alice Doe",
                Password = "ValidPassword123!",
                RoleId = 3,
                DesignationId = 1,
                Age = 30
            };

            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(request, context, results, validateAllProperties: true);

            Assert.True(isValid);
            Assert.Empty(results);
        }
    }
}
