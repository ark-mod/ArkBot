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
using ArkBot.ViewModel;
using ArkBot.Ark;
using Newtonsoft.Json;

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
                .Parameter("optional", ParameterType.Multiple)
                .Hide();
        }

        public void Init(Discord.DiscordClient client) { }

        public async Task Run(CommandEventArgs e)
        {
            if (!e.Channel.IsPrivate) return;

            var args = CommandHelper.ParseArgs(e, new { logs = false, json = false, save = false, database = false, stats = false, clear = false, keys = false, key = "", data = "" }, x =>
                x.For(y => y.logs, flag: true)
                .For(y => y.json, flag: true)
                .For(y => y.save, flag: true)
                .For(y => y.database, flag: true)
                .For(y => y.stats, flag: true)
                .For(y => y.clear, flag: true)
                .For(y => y.keys, flag: true)
                .For(y => y.key, untilNextToken: true)
                .For(y => y.data, untilNextToken: true));
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

                if (args.clear)
                {
                    foreach (var file in files) File.Delete(file);

                    await e.Channel.SendMessage("Cleared all log files!");
                    return;
                }
                else
                {
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
            }
            else if (args.keys)
            {
                if (_config.Servers != null)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("**Server instance keys**");

                    foreach (var server in _config.Servers) sb.AppendLine($"● **{server.Key}** (cluster **{server.Cluster}**): {server.Ip}:{server.Port}");

                    await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
                }
                else
                {
                    await e.Channel.SendMessage("There are no server instances configured.");
                    return;
                }
            }
            else if (args.json)
            {
                if (_config.DisableDeveloperFetchSaveData)
                {
                    await e.Channel.SendMessage("The administrator have disabled this feature.");
                    return;
                }

                var files = new List<string>();
                var tempfiles = new List<string>();
                if (!string.IsNullOrEmpty(args.key))
                {
                    //todo: no cluster data support
                    ArkServerContext server = null;
                    if (!(Workspace.Instance.ServerContexts?.TryGetValue(args.key, out server) == true) || server == null)
                    {
                        await e.Channel.SendMessage("The key did not exist.");
                        return;
                    }

                    var data = args.data?.Split(',', ';', '|');
                    if (!(data?.Length > 0))
                    {
                        await e.Channel.SendMessage("No data items supplied.");
                        return;
                    }

                    foreach (var d in data)
                    {
                        var key = d.Trim().ToLower();
                        dynamic obj = null;
                        switch (key)
                        {
                            case "wild":
                                obj = server.WildCreatures;
                                break;
                            case "tamed":
                                obj = server.TamedCreatures;
                                break;
                            case "players":
                                obj = server.Players;
                                break;
                            case "tribes":
                                obj = server.Tribes;
                                break;
                        }

                        if (obj != null)
                        {
                            var json = JsonConvert.SerializeObject(obj);
                            var jsonPath = Path.Combine(_config.TempFileOutputDirPath, $"json_{key}_" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".json");
                            tempfiles.Add(jsonPath);
                            files.Add(jsonPath);
                            File.WriteAllText(jsonPath, json);
                        }
                    }
                }
                else
                {
                    var pattern = "*.json";
                    var _files = Directory.GetFiles(_config.JsonOutputDirPath, pattern, SearchOption.AllDirectories);
                    if (_files == null || _files.Length <= 0)
                    {
                        await e.Channel.SendMessage("Could not find any json files... :(");
                        return;
                    }
                    files.AddRange(_files);
                }

                var path = Path.Combine(_config.TempFileOutputDirPath, "json_" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
                string[] results = null;
                try
                {
                    results = FileHelper.CreateDotNetZipArchive(new[] { new Tuple<string, string, string[]>(_config.JsonOutputDirPath, "", files.ToArray()) }, path, 5 * 1024 * 1024);
                    tempfiles.AddRange(results);
                    foreach (var item in results) await e.Channel.SendFile(item);
                }
                catch
                {
                    await e.Channel.SendMessage("Failed to archive json files... :(");
                    return;
                }
                finally
                {
                    foreach (var p in tempfiles) if (File.Exists(p)) File.Delete(p);
                }

                await Task.Delay(500);
                await e.Channel.SendMessage($"[parts {results.Length}]");
            }
            else if (args.save)
            {
                if (_config.DisableDeveloperFetchSaveData)
                {
                    await e.Channel.SendMessage("The administrator have disabled this feature.");
                    return;
                }

                var saveFilePath = _config.SaveFilePath;
                var clusterSavePath = _config.ClusterSavePath;
                if (!string.IsNullOrEmpty(args.key))
                {
                    var server = _config.Servers?.FirstOrDefault(x => x.Key.Equals(args.key, StringComparison.OrdinalIgnoreCase));
                    if (server == null)
                    {
                        await e.Channel.SendMessage("The key did not exist.");
                        return;
                    }
                    saveFilePath = server.SaveFilePath;
                    var cluster = !string.IsNullOrEmpty(server.Cluster) ? _config.Clusters?.FirstOrDefault(x => x.Key.Equals(server.Cluster, StringComparison.OrdinalIgnoreCase)) : null;
                    clusterSavePath = cluster?.SavePath;
                }

                var dir = Path.GetDirectoryName(saveFilePath);
                var files = new[]
                {
                    File.Exists(saveFilePath) ? new Tuple<string, string, string[]>("", "", new [] { saveFilePath }) : null,
                    Directory.Exists(dir) ? new Tuple<string, string, string[]>("", "", Directory.GetFiles(dir, "*.arkprofile", SearchOption.TopDirectoryOnly)) : null,
                    Directory.Exists(dir) ? new Tuple<string, string, string[]>("", "", Directory.GetFiles(dir, "*.arktribe", SearchOption.TopDirectoryOnly)) : null,
                    !string.IsNullOrEmpty(clusterSavePath) ? new Tuple<string, string, string[]>(clusterSavePath, "cluster", Directory.GetFiles(clusterSavePath, "*", SearchOption.AllDirectories)) : null
                }.Where(x => x != null && x.Item2 != null).ToArray();
                if (files == null || !files.Any(x => x.Item2 != null && x.Item2.Length > 0))
                {
                    await e.Channel.SendMessage("Could not find any save files... :(");
                    return;
                }

                var path = Path.Combine(_config.TempFileOutputDirPath, "save_" + (!string.IsNullOrEmpty(args.key) ? $"{args.key}_" : "") + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
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

                await Task.Delay(500);
                await e.Channel.SendMessage($"[parts {results.Length}]");
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