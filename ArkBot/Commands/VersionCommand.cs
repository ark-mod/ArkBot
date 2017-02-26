using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Helpers;
using ArkBot.Extensions;
using static System.FormattableString;
using System.Drawing;
using System.Text.RegularExpressions;
using QueryMaster.GameServer;
using System.Runtime.Caching;
using System.Reflection;

namespace ArkBot.Commands
{
    public class VersionCommand : ICommand
    {
        public string Name => "version";
        public string[] Aliases => null;
        public string Description => "Get the bot version number";
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        public void Register(CommandBuilder command) { }

        public async Task Run(CommandEventArgs e)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"**My operational instructions indicate that I am version {Assembly.GetExecutingAssembly().GetName().Version}. But what does it mean?**");

            await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
        }
    }
}
