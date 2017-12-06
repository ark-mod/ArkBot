using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Discord.Command
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
