using System;
using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.Common.Validators
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NotEqualToAttribute : ValidationAttribute
    {
        private readonly string _otherProperty;

        public NotEqualToAttribute(string otherProperty)
            : base("{0} must not be equal to {1}.")
        {
            _otherProperty = otherProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var otherPropertyInfo = validationContext.ObjectType.GetProperty(_otherProperty);
            if (otherPropertyInfo == null)
            {
                return new ValidationResult($"Unknown property: {_otherProperty}");
            }

            var otherValue = otherPropertyInfo.GetValue(validationContext.ObjectInstance);
            if (Equals(value, otherValue))
            {
                return new ValidationResult(
                    FormatErrorMessage(validationContext.DisplayName),
                    new[] { validationContext.MemberName ?? string.Empty }
                );
            }

            return ValidationResult.Success;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, name, _otherProperty);
        }
    }
}
