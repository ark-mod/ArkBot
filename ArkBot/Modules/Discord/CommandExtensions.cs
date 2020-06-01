using ArkBot.Modules.Discord.Attributes;
using Discord.Commands;
using System.Linq;

namespace ArkBot.Modules.Discord
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
