using System;

namespace ArkBot.Modules.Discord.Attributes
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
