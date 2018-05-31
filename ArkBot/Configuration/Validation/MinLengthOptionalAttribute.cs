using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;

namespace ArkBot.Configuration.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class MinLengthOptionalAttribute : IfValidationAttribute
    {
        public bool Optional { get; set; }

        private MinLengthAttribute _attr;

        public MinLengthOptionalAttribute(int length)
        {
            _attr = new MinLengthAttribute(length);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (Optional && value == null) return ValidationResult.Success;

            var result = IfMethodValid(value, validationContext);
            if (result.Item1 != IfValidResult.ContinueValidation) return result.Item2;

            return _attr.IsValid(value)
                ? ValidationResult.Success
                : new ValidationResult(String.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                    validationContext.DisplayName ?? validationContext.MemberName), new[] { validationContext.MemberName });
        }
    }
}
