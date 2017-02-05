using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Helpers
{
    public static class ValidationHelper
    {
        public static string GetDescriptionForMember<T>(T obj, string memberName, BindingFlags flags = BindingFlags.Default)
        {
            var member = obj.GetType().GetMember(memberName, flags)?.FirstOrDefault();
            if (member == null) return null;

            var attr = member.GetCustomAttribute<DescriptionAttribute>(false);
            if (attr == null) return null;

            return attr.Description;
        }
    }
}
