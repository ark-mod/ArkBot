using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Configuration.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class FileExistsAttribute : IfValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = base.IfMethodValid(value, validationContext);
            if (result.Item1 != IfValidResult.ContinueValidation) return result.Item2;

            return !string.IsNullOrWhiteSpace(value as string) && File.Exists(Environment.ExpandEnvironmentVariables((string) value))
                ? ValidationResult.Success
                : new ValidationResult(String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                    validationContext.DisplayName ?? validationContext.MemberName), new [] { validationContext.MemberName });
        }
    }
}
