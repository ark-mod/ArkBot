using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace ArkBot.Utils.Helpers
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
