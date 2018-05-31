using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Helpers;
using System.IO;
using System.Reflection;
using ArkBot.Ark;
using ArkBot.Database;
using ArkBot.Discord.Command;
using Newtonsoft.Json;
using Discord;
using Discord.Commands.Builders;
using Discord.Net;
using RestSharp;
using ArkBot.Configuration.Model;

namespace ArkBot.Commands.Experimental
{
    public class DebugCommand : ModuleBase<SocketCommandContext>
    {
        private IConfig _config;
        private IConstants _constants;
        private ArkContextManager _contextManager;

        public DebugCommand(IConfig config, IConstants constants, ArkContextManager contextManager)
        {
            _config = config;
            _constants = constants;
            _contextManager = contextManager;
        }

        [CommandHidden]
        [Command("debug")]
        [Summary("Get debug information from bot")]
        [SyntaxHelp(null)]
        [UsageExamples(null)]
        [RoleRestrictedPrecondition("debug")]
        public async Task Debug([Remainder] string arguments = null)
        {
            var args = CommandHelper.ParseArgs(arguments, new { logs = false, json = false, save = false, database = false, stats = false, clear = false, key = "", data = "" }, x =>
                x.For(y => y.logs, flag: true)
                .For(y => y.json, flag: true)
                .For(y => y.save, flag: true)
                .For(y => y.database, flag: true)
                .For(y => y.stats, flag: true)
                .For(y => y.clear, flag: true)
                .For(y => y.key, untilNextToken: true)
                .For(y => y.data, untilNextToken: true));
            if (args == null)
            {
                var syntaxHelp = MethodBase.GetCurrentMethod().GetCustomAttribute<SyntaxHelpAttribute>()?.SyntaxHelp;
                var name = MethodBase.GetCurrentMethod().GetCustomAttribute<CommandAttribute>()?.Text;

                await Context.Channel.SendMessageAsync(string.Join(Environment.NewLine, new string[] {
                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
                    !string.IsNullOrWhiteSpace(syntaxHelp) ? $"Help me by following this syntax: **!{name}** {syntaxHelp}" : null }.Where(x => x != null)));
                return;
            }

            if (args.stats)
            {
                var sb = new StringBuilder();
                sb.AppendLine("**ARK Survival Discord Bot Statistics**");
                if (File.Exists(_constants.DatabaseFilePath)) sb.AppendLine($"● **Database size:** {FileHelper.ToFileSize(new FileInfo(_constants.DatabaseFilePath).Length)}");

                await CommandHelper.SendPartitioned(Context.Channel, sb.ToString());
            }
            else if (args.logs)
            {
                var pattern = "*.log";

                var files = Directory.GetFiles(".\\logs\\", pattern, SearchOption.TopDirectoryOnly);
                if (!(files?.Length > 0))
                {
                    await Context.Channel.SendMessageAsync("Could not find any logs... :(");
                    return;
                }

                if (args.clear)
                {
                    foreach (var file in files) File.Delete(file);

                    await Context.Channel.SendMessageAsync("Cleared all log files!");
                    return;
                }

                var path = Path.Combine(_config.TempFileOutputDirPath, "logs_" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
                try
                {
                    FileHelper.CreateZipArchive(files, path);
                    await Context.Channel.SendFileAsync(path);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Failed to archive log files... :(");
                    return;
                }
                finally
                {
                    if (File.Exists(path)) File.Delete(path);
                }
            }
            else if (args.json)
            {
                if (_config.Discord.DisableDeveloperFetchSaveData)
                {
                    await Context.Channel.SendMessageAsync("The administrator have disabled this featurContext.");
                    return;
                }

                var files = new List<string>();
                var tempfiles = new List<string>();
                var basepath = (string)null;
                if (!string.IsNullOrEmpty(args.key))
                {
                    //todo: no cluster data support
                    var server = _contextManager.GetServer(args.key);
                    if (server == null)
                    {
                        await Context.Channel.SendMessageAsync("The key did not exist.");
                        return;
                    }

                    var data = args.data?.Split(',', ';', '|');
                    if (!(data?.Length > 0))
                    {
                        await Context.Channel.SendMessageAsync("No data items supplied.");
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
                            case "items":
                                obj = server.Items;
                                break;
                            case "structures":
                                obj = server.Structures;
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

                    basepath = _config.TempFileOutputDirPath;
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"**This command must include a valid server instance key.**");
                    return;
                }

                var path = Path.Combine(_config.TempFileOutputDirPath, "json_" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
                string[] results = null;
                try
                {
                    results = FileHelper.CreateDotNetZipArchive(new[] { new Tuple<string, string, string[]>(basepath, "", files.ToArray()) }, path, 5 * 1024 * 1024);
                    tempfiles.AddRange(results);
                    foreach (var item in results) await Context.Channel.SendFileAsync(item);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Failed to archive json files... :(");
                    return;
                }
                finally
                {
                    foreach (var p in tempfiles) if (File.Exists(p)) File.Delete(p);
                }

                await Task.Delay(500);
                await Context.Channel.SendMessageAsync($"[parts {results.Length}]");
            }
            else if (args.save)
            {
                if (_config.Discord.DisableDeveloperFetchSaveData)
                {
                    await Context.Channel.SendMessageAsync("The administrator have disabled this featurContext.");
                    return;
                }

                var saveFilePath = (string)null;
                var clusterSavePath = (string)null;
                if (!string.IsNullOrEmpty(args.key))
                {
                    var server = _config.Servers?.FirstOrDefault(x => x.Key.Equals(args.key, StringComparison.OrdinalIgnoreCase));
                    if (server == null)
                    {
                        await Context.Channel.SendMessageAsync("The server instance key did not exist.");
                        return;
                    }
                    saveFilePath = server.SaveFilePath;
                    var cluster = !string.IsNullOrEmpty(server.ClusterKey) ? _config.Clusters?.FirstOrDefault(x => x.Key.Equals(server.ClusterKey, StringComparison.OrdinalIgnoreCase)) : null;
                    clusterSavePath = cluster?.SavePath;
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"**This command must include a valid server instance key.**");
                    return;
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
                    await Context.Channel.SendMessageAsync("Could not find any save files... :(");
                    return;
                }

                var path = Path.Combine(_config.TempFileOutputDirPath, "save_" + (!string.IsNullOrEmpty(args.key) ? $"{args.key}_" : "") + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
                string[] results = null;
                try
                {
                    results = FileHelper.CreateDotNetZipArchive(files, path, 5 * 1024 * 1024);
                    foreach (var item in results) await Context.Channel.SendFileAsync(item);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Failed to archive save files... :(");
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
                await Context.Channel.SendMessageAsync($"[parts {results.Length}]");
            }
            else if (args.database)
            {
                var file = File.Exists(_constants.DatabaseFilePath) ? _constants.DatabaseFilePath : null;
                if (file == null)
                {
                    await Context.Channel.SendMessageAsync("Could not find the database filContext... :(");
                    return;
                }

                var path = Path.Combine(_config.TempFileOutputDirPath, "database_" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
                string[] results = null;
                try
                {
                    results = FileHelper.CreateDotNetZipArchive(new[] { new Tuple<string, string, string[]>("", "", new[] { file }) }, path, 5 * 1024 * 1024);
                    foreach (var item in results) await Context.Channel.SendFileAsync(item);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync("Failed to archive database filContext... :(");
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