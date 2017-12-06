using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Discord.Command
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SyntaxHelpAttribute : Attribute
    {
        public string SyntaxHelp { get; set; }

        public SyntaxHelpAttribute(string syntaxHelp)
        {
            SyntaxHelp = syntaxHelp;
        }
    }
}
