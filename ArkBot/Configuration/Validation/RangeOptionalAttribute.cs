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
            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name);
        }
    }
}
