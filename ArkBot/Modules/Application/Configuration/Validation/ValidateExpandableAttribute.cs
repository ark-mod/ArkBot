using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ArkBot.Modules.Application.Configuration.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ValidateExpandableAttribute : ValidationAttribute
    {

        public ValidateExpandableAttribute() : base()
        {
        }

        public override bool IsValid(object value)
        {
            var ei = value as INotifyDataErrorInfo;
            if (ei == null) return true;

            var fi = value.GetType().GetField("validationTemplate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            (fi?.GetValue(value) as ValidationTemplate)?.Validate();

            if (!ei.HasErrors) return true;

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name);
        }
    }
}
