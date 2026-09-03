using MyBackend.Application.Common.Validators;
using Xunit;

namespace MyBackend.UnitTests.Application
{
    public class PasswordValidatorTests
    {
        [Theory]
        [InlineData("Pass1234!", true)]
        [InlineData("Strong@Password2026", true)]
        [InlineData("short", false)]
        [InlineData("alllowercase123!", false)]
        [InlineData("ALLUPPERCASE123!", false)]
        [InlineData("NoSpecialChars123", false)]
        public void Validate_ShouldReturnExpectedResult(string password, bool expectedValid)
        {
            var (isValid, _) = PasswordValidator.Validate(password);

            Assert.Equal(expectedValid, isValid);
        }

        [Fact]
        public void Evaluate_WithValidStrongPassword_ShouldHaveHighScore()
        {
            var result = PasswordValidator.Evaluate("SuperSecure@2026!");

            Assert.True(result.IsValid);
            Assert.True(result.Score >= 80);
        }
    }
}
