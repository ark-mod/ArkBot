using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ArkBot.Configuration.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public sealed class RegularExpressionCustomAttribute : IfValidationAttribute
    {
        private object _typeId = new object();
        public override object TypeId => _typeId;

        public bool Optional { get; set; }

        private string _pattern;

        public RegularExpressionCustomAttribute(string pattern)
        {
            _pattern = pattern;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (Optional && value == null) return ValidationResult.Success;

            var result = IfMethodValid(value, validationContext);
            if (result.Item1 != IfValidResult.ContinueValidation) return result.Item2;

            return value != null && Regex.IsMatch(value as string, _pattern)
                ? ValidationResult.Success
                : new ValidationResult(String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                    validationContext.DisplayName ?? validationContext.MemberName), new[] { validationContext.MemberName });
        }
    }
}
