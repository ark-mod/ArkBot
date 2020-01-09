using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ArkBot.Configuration.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ValidateCollectionAttribute : ValidationAttribute
    {

        public ValidateCollectionAttribute() : base()
        {
        }

        public override bool IsValid(object value)
        {
            if (value is System.Collections.ICollection)
            {
                var collection = value as System.Collections.ICollection;
                foreach (var item in collection)
                {
                    var ei = item as INotifyDataErrorInfo;
                    if (ei == null || !ei.HasErrors) continue;

                    return false;
                }
            }

            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name);
        }
    }
}
