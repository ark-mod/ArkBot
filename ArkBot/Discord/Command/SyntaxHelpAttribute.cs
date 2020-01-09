using System;

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
