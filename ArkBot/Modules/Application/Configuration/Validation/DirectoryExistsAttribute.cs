using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;

namespace ArkBot.Modules.Application.Configuration.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class DirectoryExistsAttribute : IfValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = IfMethodValid(value, validationContext);
            if (result.Item1 != IfValidResult.ContinueValidation) return result.Item2;
            return !string.IsNullOrWhiteSpace(value as string) && Directory.Exists(Environment.ExpandEnvironmentVariables((string)value))
                ? ValidationResult.Success
                : new ValidationResult(string.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                    validationContext.DisplayName ?? validationContext.MemberName), new[] { validationContext.MemberName });
        }
    }
}
