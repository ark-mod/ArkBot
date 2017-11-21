//extern alias DotNetZip;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Discord.Commands;
//using ArkBot.Helpers;
//using ArkBot.Extensions;
//using static System.FormattableString;
//using System.Drawing;
//using System.Text.RegularExpressions;
//using QueryMaster.GameServer;
//using System.Runtime.Caching;
//using System.Globalization;
//using System.Windows.Forms.DataVisualization.Charting;
//using System.IO;
//using System.IO.Compression;
//using Autofac;
//using Discord;

//namespace ArkBot.Commands.Admin
//{
//    public class AdminCheckCommand : IRoleRestrictedCommand
//    {
//        public string Name => "check";
//        public string[] Aliases => null;
//        public string Description => "General commands to facilitate administrative tasks.";
//        public string SyntaxHelp => "<option (***resources/kibbles/eggs/structures/blueprints/base/tribelog/engrams/id/steamid***)> [***tribe <name>***] [***player <name>***]";
//        public string[] UsageExamples => new[]
//        {
//            "**id player lisa**: Find ARK ID and Steam ID for a ***player 'lisa'***",
//            "**tribelog tribe epic raw**: Get tribelogs for ***tribe 'epic'*** (get ***raw***)",
//            "**kibbles player lisa**: Listing of kibbles and eggs owned by ***player 'lisa'*** or the players tribe",
//            "**eggs tribe epic**: Listing of kibbles and eggs owned by ***tribe 'epic'***",
//            "**resources tribe epic**: Listing of resources owned by ***tribe 'epic'***",
//        };

//        public bool DebugOnly => false;
//        public bool HideFromCommandList => false;

//        public string[] ForRoles => new[] { _config.AdminRoleName, _config.DeveloperRoleName };

//        private IConfig _config;
//        private MyKibblesCommand _mykibbles;
//        private MyResourcesCommand _myresources;

//        public AdminCheckCommand(ILifetimeScope scope, IConfig config, 
//            MyKibblesCommand mykibbles, MyResourcesCommand myresources) //commands that provide functionality that admins can access for any user
//        {
//            _config = config;
//            _mykibbles = mykibbles;
//            _myresources = myresources;
//        }

//        public void Register(CommandBuilder command)
//        {
//            command.Parameter("optional", ParameterType.Multiple);
//        }

//        public void Init(DiscordClient client) { }

//        public async Task Run(CommandEventArgs e)
//        {
//            if (!e.Channel.IsPrivate) return;

//            var args = CommandHelper.ParseArgs(e, new
//            {
//                resources = false, kibbles = false, eggs = false, structures = false, blueprints = false, @base = false,
//                tribelog = false, tribelogs = false, engrams = false, id = false, ids = false, steamid = false,
//                noprefix = "", player = "", tribe = "", raw = false
//            }, x =>
//                x.For(y => y.resources, flag: true)
//                .For(y => y.kibbles, flag: true)
//                .For(y => y.eggs, flag: true)
//                .For(y => y.structures, flag: true)
//                .For(y => y.blueprints, flag: true)
//                .For(y => y.@base, flag: true)
//                .For(y => y.tribelog, flag: true)
//                .For(y => y.tribelogs, flag: true)
//                .For(y => y.engrams, flag: true)
//                .For(y => y.id, flag: true)
//                .For(y => y.ids, flag: true)
//                .For(y => y.steamid, flag: true)
//                .For(y => y.noprefix, noPrefix: true, untilNextToken: true)
//                .For(y => y.player, untilNextToken: true)
//                .For(y => y.tribe, untilNextToken: true)
//                .For(y => y.raw, flag: true));

//            var playerandtribe = args != null && (args.resources || args.kibbles || args.eggs || args.structures || args.blueprints || args.@base);
//            var playeronly = args != null && (args.id || args.ids || args.steamid || args.engrams);
//            var tribeonly = args != null && (args.tribelog || args.tribelogs);
//            if (args == null
//                || (playerandtribe && (string.IsNullOrWhiteSpace(args.player) && string.IsNullOrWhiteSpace(args.tribe)))
//                || (playeronly && string.IsNullOrWhiteSpace(args.player) && string.IsNullOrWhiteSpace(args.noprefix))
//                || (tribeonly && string.IsNullOrWhiteSpace(args.tribe) && string.IsNullOrWhiteSpace(args.noprefix)))
//            {
//                await e.Channel.SendMessage(string.Join(Environment.NewLine, new string[] {
//                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
//                    !string.IsNullOrWhiteSpace(SyntaxHelp) ? $"Help me by following this syntax: **!{Name}** {SyntaxHelp}" : null }.Where(x => x != null)));
//                return;
//            }

//            var pname = args.player ?? (playeronly ? args.noprefix : null);
//            var tname = args.tribe ?? (tribeonly ? args.noprefix : null);

//            var player = new Lazy<Data.Player>(() =>
//                !string.IsNullOrEmpty(pname) ? _context.Players?.FirstOrDefault(x => (x?.Name != null && x.Name.Equals(pname, StringComparison.OrdinalIgnoreCase)) || (x?.PlayerName != null && x.PlayerName.Equals(pname, StringComparison.OrdinalIgnoreCase))) : null);
//            var tribe = new Lazy<Data.Tribe>(() =>
//                !string.IsNullOrEmpty(tname) ? _context.Tribes?.FirstOrDefault(x => x?.Name != null && x.Name.Equals(tname, StringComparison.OrdinalIgnoreCase)) : null);
//            var playertribeortribe = new Lazy<Data.Tribe>(() => 
//                player.Value != null ? (player.Value.TribeId != null ? _context.Tribes?.FirstOrDefault(x => x.Id == player.Value.TribeId.Value) : null) : (!string.IsNullOrEmpty(tname) ? _context.Tribes?.FirstOrDefault(x => x?.Name != null && x.Name.Equals(tname, StringComparison.OrdinalIgnoreCase)) : null));
//            var namelabel = new Lazy<string>(() => 
//                (playerandtribe || playeronly) && playertribeortribe.IsValueCreated && playertribeortribe.Value?.Name != null ? playertribeortribe.Value?.Name
//                : (tribeonly) && tribe.IsValueCreated && tribe.Value?.Name != null ? tribe.Value?.Name
//                : (playerandtribe || playeronly) && player.IsValueCreated && player.Value?.Name != null ? player.Value.Name 
//                : playerandtribe ? "Player/Tribe" : tribeonly ? "Tribe" : playeronly ? "Player" : "Entity");

//            var sb = new StringBuilder();
//            if (args.id || args.ids || args.steamid)
//            {
//                var name = args.player ?? args.noprefix;
//                var matches = _context.Players?.Where(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || x.PlayerName.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
//                if (matches.Length == 0) sb.AppendLine("**Could not find the specified player name...**");
//                else
//                {
//                    sb.AppendLine("**Matching players**");
//                    var playerslist = matches.Select(x => new
//                    {
//                        Steam = x.Name,
//                        Name = x.PlayerName,
//                        Tribe = x.TribeName,
//                        Id = x.Id,
//                        SteamId = x.SteamId
//                    }).ToArray();

//                    sb.AppendLine("```");
//                    sb.AppendLine(FixedWidthTableHelper.ToString(playerslist, x => x
//                        .For(y => y.Id, "Id", alignment: 1)
//                        .For(y => y.SteamId, "SteamId", alignment: 1)));
//                    sb.AppendLine("```");
//                }
//            }
//            else if (args.resources)
//            {
                
//                if (player.Value == null && playertribeortribe.Value == null) sb.AppendLine("**Could not find the specified player or tribe...**");
//                else
//                {
//                    var result = _myresources.Get(player.Value?.Id, playertribeortribe.Value?.Id);
//                    if (result == null) sb.AppendLine($"**It appears that {namelabel.Value} do not have any Resources...**");
//                    else
//                    {
//                        sb.AppendLine($"**{namelabel.Value} have these Resources**");
//                        sb.Append(result);
//                    }
//                }
//            }
//            else if (args.kibbles || args.eggs)
//            {
//                if (player.Value == null && playertribeortribe.Value == null) sb.AppendLine("**Could not find the specified player or tribe...**");
//                else
//                {
//                    var kibbleoregg = args.eggs ? "Eggs" : "Kibbles";
//                    var result = _mykibbles.Get(player.Value?.Id, playertribeortribe.Value?.Id, args.eggs);
//                    if (result == null) sb.AppendLine($"**It appears that {namelabel.Value} do not have any {kibbleoregg}...**");
//                    else
//                    {
//                        sb.AppendLine($"**{namelabel.Value} have these {kibbleoregg }* *");
//                        sb.Append(result);
//                    }
//                }
//            }
//            else if (args.structures)
//            {
//                //not implemented
//                sb.AppendLine("**Not yet implemented... :(**");
//            }
//            else if (args.blueprints)
//            {
//                //not implemented
//                sb.AppendLine("**Not yet implemented... :(**");
//            }
//            else if (args.@base)
//            {
//                //not implemented
//                sb.AppendLine("**Not yet implemented... :(**");
//            }
//            else if (args.tribelog || args.tribelogs)
//            {
//                if (tribe.Value == null) sb.AppendLine("**Could not find the specified tribe name...**");
//                else if (!(tribe.Value.TribeLog?.Length > 0)) sb.AppendLine($"**There are no tribe logs for {namelabel.Value}...**");
//                else
//                {
//                    var path = FileHelper.GetAvailableFilePathSequential(Path.Combine(_config.TempFileOutputDirPath, "tribelog.txt"));
//                    try
//                    {
//                        if (!args.raw) File.WriteAllLines(path, tribe.Value.TribeLog.Select(x => Data.TribeLog.FromLog(x).ToStringPretty()).ToArray());
//                        else File.WriteAllLines(path, tribe.Value.TribeLog);
//                        await e.Channel.SendFile(path);
//                    }
//                    finally
//                    {
//                        if (File.Exists(path)) File.Delete(path);
//                    }
//                }
//            }
//            else if (args.engrams)
//            {
//                //not implemented
//                sb.AppendLine("**Not yet implemented... :(**");
//            }
//            var msg = sb.ToString();
//            if (!string.IsNullOrWhiteSpace(msg)) await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
//        }
//    }
//}