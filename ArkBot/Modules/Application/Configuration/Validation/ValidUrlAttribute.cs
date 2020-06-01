﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ArkBot.Modules.Application.Configuration.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ValidUrlAttribute : ValidationAttribute
    {
        public bool Optional { get; set; }

        public override bool IsValid(object value)
        {
            if (!(value is string)) return false;
            if (Optional && string.IsNullOrEmpty((string)value)) return true;

            return Uri.TryCreate((string)value, UriKind.Absolute, out var uriResult) &&
                   (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name);
        }
    }
}
