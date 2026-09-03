using System.Text.RegularExpressions;

namespace MyBackend.Application.Common.Validators
{
    public sealed class PasswordEvaluationDetails
    {
        public bool IsValid { get; set; }
        public bool IsStrong { get; set; }
        public int Score { get; set; }
        public string StrengthLabel { get; set; } = "Very Weak";
        public bool MinLength { get; set; }
        public bool HasUpper { get; set; }
        public bool HasLower { get; set; }
        public bool HasNumber { get; set; }
        public bool HasSpecial { get; set; }
        public List<string> Errors { get; set; } = [];
    }

    public static class PasswordValidator
    {
        public static (bool IsValid, List<string> Errors) Validate(string? password)
        {
            var eval = Evaluate(password);
            return (eval.IsValid, eval.Errors);
        }

        public static PasswordEvaluationDetails Evaluate(string? password)
        {
            var details = new PasswordEvaluationDetails();
            var errors = new List<string>();

            if (string.IsNullOrEmpty(password))
            {
                errors.Add("Password cannot be empty.");
                details.Errors = errors;
                details.Score = 0;
                details.StrengthLabel = "Empty";
                return details;
            }

            details.MinLength = password.Length >= 8;
            if (!details.MinLength)
            {
                errors.Add("Password must be at least 8 characters long.");
            }

            details.HasUpper = Regex.IsMatch(password, @"[A-Z]");
            if (!details.HasUpper)
            {
                errors.Add("Password must contain at least one uppercase letter (A-Z).");
            }

            details.HasLower = Regex.IsMatch(password, @"[a-z]");
            if (!details.HasLower)
            {
                errors.Add("Password must contain at least one lowercase letter (a-z).");
            }

            details.HasNumber = Regex.IsMatch(password, @"[0-9]");
            if (!details.HasNumber)
            {
                errors.Add("Password must contain at least one numeric digit (0-9).");
            }

            details.HasSpecial = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?~`]");
            if (!details.HasSpecial)
            {
                errors.Add("Password must contain at least one special character (e.g. !@#$%^&*).");
            }

            int passedCount = 0;
            if (details.MinLength) passedCount++;
            if (details.HasUpper) passedCount++;
            if (details.HasLower) passedCount++;
            if (details.HasNumber) passedCount++;
            if (details.HasSpecial) passedCount++;

            details.Score = passedCount * 20;

            details.StrengthLabel = passedCount switch
            {
                5 => "Strong",
                4 => "Good",
                3 => "Fair",
                2 => "Weak",
                _ => "Very Weak"
            };

            details.IsValid = errors.Count == 0;
            details.IsStrong = details.IsValid;
            details.Errors = errors;

            return details;
        }
    }
}
