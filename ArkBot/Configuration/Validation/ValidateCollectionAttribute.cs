using System;
using System.Collections.Generic;
using System.ComponentModel;
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
