using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Helpers;
using ArkBot.Extensions;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using Autofac;
using Discord;
using ArkBot.Services;
using ArkBot.Ark;
using ArkBot.Services.Data;
using ArkSavegameToolkitNet.Domain;
using System.Threading;
using ArkBot.Discord.Command;
using Discord.Commands.Builders;
using RestSharp;
using ArkBot.Configuration.Model;

namespace ArkBot.Commands.Admin
{
    public class CloudCommand : ModuleBase<SocketCommandContext>
    {
        private IConfig _config;
        private ISavegameBackupService _savegameBackupService;
        private ArkContextManager _contextManager;

        public CloudCommand(
            IConfig config,
            ISavegameBackupService savegameBackupService,
            ArkContextManager contextManager)
        {
            _config = config;
            _savegameBackupService = savegameBackupService;
            _contextManager = contextManager;
        }

        [CommandHidden]
        [Command("cloud")]
        [Alias("cluster")]
        [Summary("Admin commands to manage the cloud/cluster.")]
        [SyntaxHelp(null)]
        [UsageExamples(new[]
        {
            "**<cluster key> backup <steamid>**: Creates a backup of the current cloud save for the given steamid.",
            "**<cluster key> list <steamid> [skip <number>]**: List cloud save backups available for the given steamid.",
            "**<cluster key> details <steamid> <backuphash>**: View detailed information for a given cloud save backup.",
            "**<cluster key> restore <steamid> <backuphash> <cloudsavehash>**: Restore a given cloud save backup.",
            "**<cluster key> delete <steamid>**: Delete the current cloud save for the given steamid.",
            "**<cluster key> stash <steamid> <tag>**: Stash a players cloud save using the the given tag.",
            "**<cluster key> pop <steamid> <tag>**: Restore a stashed cloud save with the given tag for a player.",
        })]
        [RoleRestrictedPrecondition("cloud")]
        public async Task Cloud([Remainder] string arguments = null)
        {
            var args = CommandHelper.ParseArgs(arguments, new
            {
                ClusterKey = "",
                Backup = 0L,
                Stash = 0L,
                Pop = 0L,
                Delete = 0L,
                List = 0L,
                Details = 0L,
                Restore = 0L,
                Target1 = "",
                Target2 = "",
                Skip = 0
            }, x =>
                x.For(y => y.ClusterKey, noPrefix: true)
                .For(y => y.Target1, noPrefix: true)
                .For(y => y.Target2, noPrefix: true));

            var sb = new StringBuilder();
            var r_allowedExt = new Regex(@"^(?!tmpprofile)([a-z0-9])+$", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            //for details and restore commands, get hashes from target1 and target2
            int? backupHash = null, cloudSaveHash = null;
            if (args.Details > 0 || args.Restore > 0)
            {
                try
                {
                    backupHash = !string.IsNullOrWhiteSpace(args.Target1) ? (int?)Convert.ToInt32(args.Target1, 16) : null;
                }
                catch { /*ignore exceptions*/ }
            }
            if (args.Restore > 0)
            {
                try
                {
                    cloudSaveHash = !string.IsNullOrWhiteSpace(args.Target2) ? (int?)Convert.ToInt32(args.Target2, 16) : null;
                }
                catch { /*ignore exceptions*/ }
            }

            // for stash and pop commands, check that target1 is a valid tag
            if ((args.Stash > 0 || args.Pop > 0) && !r_allowedExt.IsMatch(args.Target1))
            {
                await Context.Channel.SendMessageAsync($"**The supplied tag is not allowed (only a-z, 0-9)!**");
                return;
            }

            // check that the cluster key is valid
            ArkClusterContext clusterContext = args.ClusterKey != null ? _contextManager.GetCluster(args.ClusterKey) : null;
            if (clusterContext == null)
            {
                await Context.Channel.SendMessageAsync($"**Cloud commands need to be prefixed with a valid cluster key.**");
                return;
            }

            // check that there are one or more servers in the cluster
            var serverContexts = _contextManager.GetServersInCluster(clusterContext.Config.Key);
            if (!(serverContexts?.Length > 0))
            {
                await Context.Channel.SendMessageAsync($"**There are no servers in the cluster.**");
                return;
            }

            /* ---------------------------------------------------------------
               List cloud save backups available for a given player.
               --------------------------------------------------------------- */
            if (args.List > 0)
            {
                var result = GetBackupFiles(clusterContext.Config, serverContexts.Select(x => x.Config.Key).ToArray(), args.List);

                if (result.Count > 0)
                {
                    var tbl = OutputCloudBackupListingTable(result, args.Skip);
                    sb.Append(tbl);
                }
                else sb.AppendLine("**Could not find any cloud save backups...**");
            }

            /* ---------------------------------------------------------------
               Stash the current cloud save with a given tag to fetch at a later time.
               --------------------------------------------------------------- */
            else if (args.Stash > 0)
            {
                var result = _savegameBackupService.StashCloudSave(clusterContext.Config, args.Stash, args.Target1);
                if (result == StashResult.Successfull) sb.AppendLine($"**Cloud save stashed as '{args.Target1}'!**");
                else if (result == StashResult.SourceMissing) sb.AppendLine("**There is no cloud save to stash...**");
                else if (result == StashResult.TargetExists) sb.AppendLine("**The supplied tag is already being used...**");
                else sb.AppendLine("**Failed to stash cloud save...**");
            }

            /* ---------------------------------------------------------------
               Fetch a cloud save previously stashed with a given tag and set it as the current cloud save.
               --------------------------------------------------------------- */
            else if (args.Pop > 0)
            {
                var result = _savegameBackupService.PopCloudSave(clusterContext.Config, args.Pop, args.Target1);
                if (result == StashResult.Successfull) sb.AppendLine($"**Cloud save popped from '{args.Target1}'!**");
                else if (result == StashResult.SourceMissing) sb.AppendLine("**The supplied tag does not exist...**");
                else if (result == StashResult.TargetExists) sb.AppendLine("**A cloud save already exists, please delete/stash it before popping...**");
                else sb.AppendLine("**Failed to pop cloud save...**");
            }

            /* ---------------------------------------------------------------
               Delete the current cloud save for a given player.
               --------------------------------------------------------------- */
            else if (args.Delete > 0)
            {
                var targetPath = Path.Combine(clusterContext.Config.SavePath, $"{args.Delete}");
                if (File.Exists(targetPath))
                {
                    try
                    {
                        File.Delete(targetPath);
                        sb.AppendLine($"**Cloud save deleted!**");
                    }
                    catch
                    {
                        sb.AppendLine($"**Failed to delete cloud save...**");
                    }
                }
                else sb.AppendLine($"**There is no cloud save to delete...**");
            }

            /* ---------------------------------------------------------------
               Create a backup of all cloud save files for a given player (including stashed, .tmpprofile etc.)
               --------------------------------------------------------------- */
            else if (args.Backup > 0)
            {
                var result = _savegameBackupService.CreateClusterBackupForSteamId(clusterContext.Config, args.Backup);
                if (result != null && result.ArchivePaths?.Length > 0) sb.AppendLine($"**Cloud save backup successfull!**");
                else sb.AppendLine("**Failed to backup cloud save...**");
            }

            /* ---------------------------------------------------------------
               Get detailed information for a single backup archive or cloud save file.
               --------------------------------------------------------------- */
            else if (args.Details > 0 && backupHash.HasValue)
            {
                var result = GetBackupFiles(clusterContext.Config, serverContexts.Select(x => x.Config.Key).ToArray(), args.Details, backupHash.Value)
                    .Find(x => x.Path.GetHashCode() == backupHash.Value);

                if (result == null)
                {
                    await Context.Channel.SendMessageAsync($"**Failed to find the given backup hash!**");
                    return;
                }

                var data = result.Files.Select(file =>
                {
                    string tmpFilePath = null;
                    ArkCloudInventory cloudInventory = null;
                    try
                    {
                        string filePath = null;
                        if (result is FromServerBackupListEntity) filePath = result.FullPath;
                        else filePath = tmpFilePath = FileHelper.ExtractFileInZipFile(result.FullPath, file);

                        var cresult = ArkClusterData.LoadSingle(filePath, CancellationToken.None, true, true);
                        cloudInventory = cresult.Success ? cresult.Data : null;
                    }
                    catch
                    {
                        /*ignore exception*/
                    }
                    finally
                    {
                        if (tmpFilePath != null) File.Delete(tmpFilePath);
                    }

                    return new
                    {
                        FilePath = file,
                        CloudSaveHash = file.GetHashCode(),
                        DinoCount = cloudInventory?.Dinos?.Length,
                        CharactersCount = cloudInventory?.Characters?.Length,
                        ItemsCount = cloudInventory?.Items?.Length
                    };
                }).ToArray();

                var tableBackupFiles = FixedWidthTableHelper.ToString(data, x => x
                        .For(y => y.FilePath, header: "Cloud Save")
                        .For(y => y.CloudSaveHash, header: "Cloud Save Hash", alignment: 1, format: "X")
                        .For(y => y.DinoCount, header: "Dinos", alignment: 1)
                        .For(y => y.CharactersCount, header: "Characters", alignment: 1)
                        .For(y => y.ItemsCount, header: "Items", alignment: 1));

                var tableBackupEntries = OutputCloudBackupListingTable(new[] { result }, 0, 1);

                sb.Append(tableBackupEntries);
                sb.Append($"```{tableBackupFiles}```");
            }

            /* ---------------------------------------------------------------
              Restore a single cloud save file from a backup archive or cloud save file (some overlap with the less verbose pop command).
               --------------------------------------------------------------- */
            else if (args.Restore > 0 && backupHash.HasValue && cloudSaveHash.HasValue)
            {
                var result = GetBackupFiles(clusterContext.Config, serverContexts.Select(x => x.Config.Key).ToArray(), args.Restore, backupHash.Value)
                    .Find(x => x.Path.GetHashCode() == backupHash.Value);

                if (result == null)
                {
                    await Context.Channel.SendMessageAsync($"**Failed to find the given backup hash!**");
                    return;
                }

                var cloudSaveFile = result.Files.FirstOrDefault(x => x.GetHashCode() == cloudSaveHash.Value);
                if (cloudSaveFile == null)
                {
                    await Context.Channel.SendMessageAsync($"**Failed to find the given cloud save hash!**");
                    return;
                }

                var targetPath = Path.Combine(clusterContext.Config.SavePath, $"{args.Restore}");
                if (File.Exists(targetPath))
                {
                    await Context.Channel.SendMessageAsync("**A cloud save already exists, please delete/stash it before restoring...**");
                    return;
                }

                string tmpFilePath = null;
                try
                {
                    string filePath = null;
                    if (result is FromServerBackupListEntity) filePath = result.FullPath;
                    else filePath = tmpFilePath = FileHelper.ExtractFileInZipFile(result.FullPath, cloudSaveFile);

                    File.Copy(filePath, targetPath);

                    sb.AppendLine($"**Cloud save successfully restored!**");
                }
                catch
                {
                    /*ignore exception*/
                    sb.AppendLine($"**Failed to restore cloud save...**");
                }
                finally
                {
                    if (tmpFilePath != null) File.Delete(tmpFilePath);
                }
            }
            else
            {
                var syntaxHelp = MethodBase.GetCurrentMethod().GetCustomAttribute<SyntaxHelpAttribute>()?.SyntaxHelp;
                var name = MethodBase.GetCurrentMethod().GetCustomAttribute<CommandAttribute>()?.Text;

                await Context.Channel.SendMessageAsync(string.Join(Environment.NewLine, new string[] {
                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
                    !string.IsNullOrWhiteSpace(syntaxHelp) ? $"Help me by following this syntax: **!{name}** {syntaxHelp}" : null }.Where(x => x != null)));
                return;
            }

            var msg = sb.ToString();
            if (!string.IsNullOrWhiteSpace(msg)) await CommandHelper.SendPartitioned(Context.Channel, sb.ToString());
        }

        /// <summary>
        /// Collect all cloud save backup file entries
        /// </summary>
        /// <returns></returns>
        List<BackupListEntity> GetBackupFiles(ClusterConfigSection clusterConfig, string[] serverKeys, long steamId, int? forHash = null)
        {
            // collect backup archives that have cloud saves for a given player
            var clusterFilePathForPlayer = $"cluster/{steamId}";
            var clusterFileFilterFunc = new Func<string, bool>(x => x.StartsWith(clusterFilePathForPlayer, StringComparison.OrdinalIgnoreCase));
            var result = _savegameBackupService.GetBackupsList(
                new[] { clusterConfig.Key }.Concat(serverKeys).ToArray(),
                (fi, be) => ((forHash.HasValue && be.Path.GetHashCode() == forHash.Value) || !forHash.HasValue) && fi.LastWriteTime >= DateTime.Now.AddDays(-7) && be.Files.Any(clusterFileFilterFunc)) ?? new List<Services.Data.BackupListEntity>();

            // filter out non cloud saves
            result.ForEach(x => x.Files = x.Files.Where(clusterFileFilterFunc).OrderBy(y => y).ToArray());

            // collect stashed/.tmpprofile/etc. cloud saves from the clusters directory on the server
            var cloudBackupFiles = _savegameBackupService.GetCloudBackupFilesForSteamId(clusterConfig, steamId);
            if (cloudBackupFiles?.Count > 0)
            {
                cloudBackupFiles.ForEach(x => x.Files = x.Files.OrderBy(y => y).ToArray());
                result.AddRange(cloudBackupFiles);
            }

            return result;
        }

        /// <summary>
        /// Convert backup list entries into a fixed width table
        /// </summary>
        string OutputCloudBackupListingTable(IEnumerable<BackupListEntity> result, int skip, int take = 25)
        {
            var data = result.OrderByDescending(x => x.DateModified).Skip(skip).Take(take).Select(x => new
            {
                Path = $"{x.Path}",
                BackupHash = x.Path.GetHashCode(),
                Age = (DateTime.Now - x.DateModified).ToStringCustom(),
                Files = x.Files.Length,
                FileSize = x.ByteSize.ToFileSize()
            }).ToArray();

            var table = FixedWidthTableHelper.ToString(data, x => x
                .For(y => y.Path, header: "Cloud Save Backup")
                .For(y => y.BackupHash, header: "Backup Hash", alignment: 1, format: "X")
                .For(y => y.Age, alignment: 1)
                .For(y => y.Files, header: "Saves", alignment: 1)
                .For(y => y.FileSize, header: "File Size", alignment: 1));

            return $"```{table}```";
        }
    }
}