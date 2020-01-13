using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Helpers
{
    public static class ValidationHelper
    {
        public static string GetDescriptionForMember<T>(T obj, string memberName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            var member = (obj != null ? obj.GetType() : typeof(T)).GetMember(memberName, flags)?.FirstOrDefault();
            if (member == null) return null;

            var attr = member.GetCustomAttribute<DescriptionAttribute>(false);
            if (attr != null) return attr.Description;
            
            var attr2 = member.GetCustomAttribute<DisplayAttribute>(false);
            if (attr2 != null) return attr2.Description;
            
            return null;
        }
    }
}
