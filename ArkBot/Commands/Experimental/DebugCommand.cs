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
        private IConstants _constants;

        public DebugCommand(IConfig config, IConstants constants)
        {
            _config = config;
            _constants = constants;
        }

        public void Register(CommandBuilder command)
        {
            command.AddCheck((a, b, c) => c.Client.Servers.Any(x => x.Roles.Any(y => y != null && y.Name.Equals(_config.DeveloperRoleName) && y.Members.Any(z => z.Id == b.Id))), null)
                .Parameter("optional", ParameterType.Optional)
                .Hide();
        }

        public void Init(Discord.DiscordClient client) { }

        public async Task Run(CommandEventArgs e)
        {
            if (!e.Channel.IsPrivate) return;

            var args = CommandHelper.ParseArgs(e, new { logs = false, json = false, save = false, database = false, stats = false }, x =>
                x.For(y => y.logs, flag: true)
                .For(y => y.json, flag: true)
                .For(y => y.save, flag: true)
                .For(y => y.database, flag: true)
                .For(y => y.stats, flag: true));
            if (args == null)
            {
                await e.Channel.SendMessage(string.Join(Environment.NewLine, new string[] {
                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
                    !string.IsNullOrWhiteSpace(SyntaxHelp) ? $"Help me by following this syntax: **!{Name}** {SyntaxHelp}" : null }.Where(x => x != null)));
                return;
            }

            if (args.stats)
            {
                var sb = new StringBuilder();
                sb.AppendLine("**ARK Survival Discord Bot Statistics**");
                if (File.Exists(_constants.DatabaseFilePath)) sb.AppendLine($"● **Database size:** {FileHelper.ToFileSize(new FileInfo(_constants.DatabaseFilePath).Length)}");

                await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
            }
            else if (args.logs)
            {
                var pattern = "*.log";

                var files = Directory.GetFiles(".\\logs\\", pattern, SearchOption.TopDirectoryOnly);
                if (files == null || files.Length <= 0)
                {
                    await e.Channel.SendMessage("Could not find any logs... :(");
                    return;
                }

                var path = Path.Combine(_config.TempFileOutputDirPath, "logs_" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
                try
                {
                    FileHelper.CreateZipArchive(files, path);
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
            else if (args.json)
            {
                if (_config.DisableDeveloperFetchSaveData)
                {
                    await e.Channel.SendMessage("The administrator have disabled this feature.");
                    return;
                }

                var pattern = "*.json";
                var files = Directory.GetFiles(_config.JsonOutputDirPath, pattern, SearchOption.AllDirectories);
                if (files == null || files.Length <= 0)
                {
                    await e.Channel.SendMessage("Could not find any json files... :(");
                    return;
                }

                var path = Path.Combine(_config.TempFileOutputDirPath, "json_" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
                try
                {
                    FileHelper.CreateDotNetZipArchive(new[] { new Tuple<string, string, string[]>(_config.JsonOutputDirPath, "", files) }, path, 5 * 1024 * 1024);
                    await e.Channel.SendFile(path);
                }
                catch
                {
                    await e.Channel.SendMessage("Failed to archive json files... :(");
                    return;
                }
                finally
                {
                    if (File.Exists(path)) File.Delete(path);
                }
            }
            else if (args.save)
            {
                if (_config.DisableDeveloperFetchSaveData)
                {
                    await e.Channel.SendMessage("The administrator have disabled this feature.");
                    return;
                }

                var dir = Path.GetDirectoryName(_config.SaveFilePath);
                var files = new[]
                {
                    File.Exists(_config.SaveFilePath) ? new Tuple<string, string, string[]>("", "", new [] { _config.SaveFilePath }) : null,
                    Directory.Exists(dir) ? new Tuple<string, string, string[]>("", "", Directory.GetFiles(dir, "*.arkprofile", SearchOption.TopDirectoryOnly)) : null,
                    Directory.Exists(dir) ? new Tuple<string, string, string[]>("", "", Directory.GetFiles(dir, "*.arktribe", SearchOption.TopDirectoryOnly)) : null,
                    new Tuple<string, string, string[]>(_config.ClusterSavePath, "cluster", Directory.GetFiles(_config.ClusterSavePath, "*", SearchOption.AllDirectories))
                }.Where(x => x != null && x.Item2 != null).ToArray();
                if (files == null || !files.Any(x => x.Item2 != null && x.Item2.Length > 0))
                {
                    await e.Channel.SendMessage("Could not find any save files... :(");
                    return;
                }

                var path = Path.Combine(_config.TempFileOutputDirPath, "save_" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
                string[] results = null;
                try
                {
                    results = FileHelper.CreateDotNetZipArchive(files, path, 5 * 1024 * 1024);
                    foreach (var item in results) await e.Channel.SendFile(item);
                }
                catch
                {
                    await e.Channel.SendMessage("Failed to archive save files... :(");
                    return;
                }
                finally
                {
                    if (results != null)
                    {
                        foreach (var item in results)
                        {
                            if (File.Exists(item)) File.Delete(item);
                        }
                    }
                }
            }
            else if (args.database)
            {
                var file = File.Exists(_constants.DatabaseFilePath) ? _constants.DatabaseFilePath : null;
                if (file == null)
                {
                    await e.Channel.SendMessage("Could not find the database file... :(");
                    return;
                }

                var path = Path.Combine(_config.TempFileOutputDirPath, "database_" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
                string[] results = null;
                try
                {
                    results = FileHelper.CreateDotNetZipArchive(new[] { new Tuple<string, string, string[]>("", "", new[] { file }) }, path, 5 * 1024 * 1024);
                    foreach (var item in results) await e.Channel.SendFile(item);
                }
                catch
                {
                    await e.Channel.SendMessage("Failed to archive database file... :(");
                    return;
                }
                finally
                {
                    if (results != null)
                    {
                        foreach (var item in results)
                        {
                            if (File.Exists(item)) File.Delete(item);
                        }
                    }
                }
            }
        }
    }
}