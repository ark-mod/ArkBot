using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using VDS.RDF;

namespace ArkBot.Discord.Command
{
    public static class CommandInfoExtensions
    {
        public static string SyntaxHelp(this CommandInfo self)
        {
            var attr = self.Attributes.OfType<SyntaxHelpAttribute>().FirstOrDefault();
            return attr?.SyntaxHelp;
        }
        public static string[] UsageExamples(this CommandInfo self)
        {
            var attr = self.Attributes.OfType<UsageExamplesAttribute>().FirstOrDefault();
            return attr?.UsageExamples;
        }
    }
}
