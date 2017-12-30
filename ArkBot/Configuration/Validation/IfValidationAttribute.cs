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
    public abstract class IfValidationAttribute : ValidationAttribute
    {
        public string IfMethod { get; set; }

        protected ValidationResult IfMethodValid(object value, ValidationContext validationContext)
        {
            if (IfMethod != null)
            {
                var ifMethodInfo = validationContext.ObjectType.GetMethod(IfMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (ifMethodInfo == null)
                    return new ValidationResult(
                        $"IfMethod '{validationContext.ObjectType.Name}.{IfMethod}' could not be found!", new[] { validationContext.MemberName });

                if (!(bool) ifMethodInfo.Invoke(validationContext.ObjectInstance, null))
                    return ValidationResult.Success;
            }

            return null;
        }
    }
}
