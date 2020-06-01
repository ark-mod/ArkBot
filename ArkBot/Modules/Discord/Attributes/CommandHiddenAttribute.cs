using System;
using System.Collections.Generic;
using System.Linq;

namespace ArkBot.Modules.Discord.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CommandHiddenAttribute : Attribute
    {
        public static bool IsHidden(IReadOnlyList<Attribute> module, IReadOnlyList<Attribute> command)
        {
            return module.Any(x => x is CommandHiddenAttribute) || command.Any(x => x is CommandHiddenAttribute);
        }
    }
}
