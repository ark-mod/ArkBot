using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ArkBot.Helpers
{
    public static class ValidationHelper
    {
        public static string GetDescriptionForMember<T>(T obj, string memberName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            var member = obj.GetType().GetMember(memberName, flags)?.FirstOrDefault();
            if (member == null) return null;

            var attr = member.GetCustomAttribute<DescriptionAttribute>(false);
            if (attr == null) return null;

            return attr.Description;
        }
    }
}
