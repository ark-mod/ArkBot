extern alias DotNetZip;
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
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.IO.Compression;
using Autofac;

namespace ArkBot.Commands.Experimental
{
    public class AdminCommand : IRoleRestrictedCommand
    {
        public string Name => "admin";
        public string[] Aliases => null;
        public string Description => "Admin commands to manage the ARK Server (rcon etc.)";
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        public string[] ForRoles => new[] { _config.AdminRoleName, _config.DeveloperRoleName };

        private IArkContext _context;
        private IConfig _config;

        public AdminCommand(ILifetimeScope scope, IArkContext context, IConfig config)
        {
            _context = context;
            _config = config;
        }

        public void Register(CommandBuilder command)
        {
            command.Parameter("optional", ParameterType.Multiple);
        }

        public void Init(Discord.DiscordClient client) { }

        public async Task Run(CommandEventArgs e)
        {
            if (!e.Channel.IsPrivate) return;

            var args = CommandHelper.ParseArgs(e, new
            {
                saveworld = false
            }, x =>
                x.For(y => y.saveworld, flag: true));

            var sb = new StringBuilder();

            if (args.saveworld)
            {
                var result = await CommandHelper.SendRconCommand(_config, "saveworld");
                if (result == null) sb.AppendLine("**Failed to save world... :(**");
                else sb.AppendLine("**World saved!**");
            }

            var msg = sb.ToString();
            if (!string.IsNullOrWhiteSpace(msg)) await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
        }
    }
}