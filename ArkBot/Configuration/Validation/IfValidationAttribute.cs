using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ArkBot.Configuration.Validation
{
    public enum IfValidResult { NotValid, Valid, ContinueValidation }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public abstract class IfValidationAttribute : ValidationAttribute
    {
        public string IfMethod { get; set; }

        protected Tuple<IfValidResult, ValidationResult> IfMethodValid(object value, ValidationContext validationContext)
        {
            if (IfMethod != null)
            {
                var ifMethodInfo = validationContext.ObjectType.GetMethod(IfMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (ifMethodInfo == null)
                    return Tuple.Create(IfValidResult.NotValid, new ValidationResult(
                        $"IfMethod '{validationContext.ObjectType.Name}.{IfMethod}' could not be found!", new[] { validationContext.MemberName }));

                if (!(bool) ifMethodInfo.Invoke(validationContext.ObjectInstance, null))
                    return Tuple.Create(IfValidResult.Valid, ValidationResult.Success);
            }

            return Tuple.Create(IfValidResult.ContinueValidation, ValidationResult.Success);
        }
    }
}
