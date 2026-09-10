using System;
using System.ComponentModel.DataAnnotations;

namespace MyBackend.Application.Common.Validators
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public class DateRangeAttribute : ValidationAttribute
    {
        private readonly string _startDateProperty;
        private readonly string _endDateProperty;

        public DateRangeAttribute(string startDateProperty, string endDateProperty)
            : base("{0} must not be later than {1}.")
        {
            _startDateProperty = startDateProperty;
            _endDateProperty = endDateProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var startProp = validationContext.ObjectType.GetProperty(_startDateProperty);
            var endProp = validationContext.ObjectType.GetProperty(_endDateProperty);

            if (startProp == null || endProp == null)
            {
                return ValidationResult.Success;
            }

            var startVal = startProp.GetValue(validationContext.ObjectInstance) as DateTime?;
            var endVal = endProp.GetValue(validationContext.ObjectInstance) as DateTime?;

            if (startVal.HasValue && endVal.HasValue && startVal.Value > endVal.Value)
            {
                return new ValidationResult(
                    FormatErrorMessage(_startDateProperty),
                    new[] { _startDateProperty, _endDateProperty }
                );
            }

            return ValidationResult.Success;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, _startDateProperty, _endDateProperty);
        }
    }
}
