using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ArkBot.Modules.Application.Configuration.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class RangeOptionalAttribute : RangeAttribute
    {
        public bool Optional { get; set; }

        public RangeOptionalAttribute(int minimum, int maximum) : base(minimum, maximum)
        {
        }

        public override bool IsValid(object value)
        {
            if (Optional && value == null) return true;

            return base.IsValid(value);
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name);
        }
    }
}
