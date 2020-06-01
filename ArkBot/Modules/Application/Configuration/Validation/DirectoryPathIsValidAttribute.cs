﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;

namespace ArkBot.Modules.Application.Configuration.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class DirectoryPathIsValidAttribute : IfValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = IfMethodValid(value, validationContext);
            if (result.Item1 != IfValidResult.ContinueValidation) return result.Item2;

            var success = !string.IsNullOrEmpty(value as string);
            if (success)
            {
                try
                {
                    Path.GetFullPath(Environment.ExpandEnvironmentVariables((string)value));
                }
                catch { success = false; }
            }

            return success
                ? ValidationResult.Success
                : new ValidationResult(string.Format(CultureInfo.CurrentCulture, ErrorMessageString,
                    validationContext.DisplayName ?? validationContext.MemberName), new[] { validationContext.MemberName });
        }
    }
}
