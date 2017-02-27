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

namespace ArkBot.Commands.Experimental
{
    public class DebugCommand : ICommand
    {
        public string Name => "debug";
        public string[] Aliases => null;
        public string Description => "Get debug information from bot";
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => false;
        public bool HideFromCommandList => true;

        private IConfig _config;

        public DebugCommand(IConfig config)
        {
            _config = config;
        }

        public void Register(CommandBuilder command)
        {
            command.AddCheck((a, b, c) => c.Client.Servers.Any(x => x.Roles.Any(y => y != null && y.Name.Equals("developer") && y.Members.Any(z => z.Id == b.Id))), null)
                .Parameter("optional", ParameterType.Optional)
                .Hide();
        }

        public async Task Run(CommandEventArgs e)
        {
            if (!e.Channel.IsPrivate) return;

            var args = CommandHelper.ParseArgs(e, new { logs = false, applicationcrash = false, exception = false }, x =>
                x.For(y => y.logs, flag: true)
                .For(y => y.applicationcrash, flag: true)
                .For(y => y.exception, flag: true));
            if (args == null)
            {
                await e.Channel.SendMessage(string.Join(Environment.NewLine, new string[] {
                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
                    !string.IsNullOrWhiteSpace(SyntaxHelp) ? $"Help me by following this syntax: **!{Name}** {SyntaxHelp}" : null }.Where(x => x != null)));
                return;
            }

            if (args.logs || args.exception || args.applicationcrash)
            {
                var pattern = "*.log";
                if (args.applicationcrash) pattern = "applicationcrash_*.log";
                else if (args.exception) pattern = "exception_*.log";

                var files = Directory.GetFiles(".\\", pattern, SearchOption.TopDirectoryOnly);
                if (files == null || files.Length <= 0)
                {
                    await e.Channel.SendMessage("Could not find any logs... :(");
                    return;
                }

                var path = Path.Combine(_config.TempFileOutputDirPath, "logs_" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
                try
                {
                    using (var fileStream = new FileStream(path, FileMode.CreateNew))
                    {
                        using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                        {
                            foreach (var file in files)
                            {
                                var entry = archive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Fastest);
                            }
                        }
                    }

                    await e.Channel.SendFile(path);
                }
                catch
                {
                    await e.Channel.SendMessage("Failed to archive log files... :(");
                    return;
                }
                finally
                {
                    if (File.Exists(path)) File.Delete(path);
                }
            }
        }
    }
}