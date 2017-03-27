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

namespace ArkBot.Commands
{
    public class FindTameCommand : ICommand
    {
        public string Name => "findtame";
        public string[] Aliases => new[] { "findtames", "findpet", "findpets" };
        public string Description => "Lets you find a tamed dino that have gone missing";
        public string SyntaxHelp => "<***name*** (minimum length 2)> [<option (***exact/species***)>] [***tribe <name>***] [***owner <name>***] [***skip <number>***]";
        public string[] UsageExamples => new[]
        {
            "**lina**: Looks for a tame using a ***partial*** name ***'lina'***",
            "**lars exact**: Looks for a tame using an ***exact*** name ***'lars'***",
            "**doedicurus species**: Looks for any tame of the ***species 'doedicurus'***",
            "**lina owner nils**: Looks for a tame using a partial name ***'lina'*** belonging to the ***player 'nils'***",
            "**lina tribe epic**: Looks for a tame using a partial name ***'lina'*** belonging to the ***tribe 'epic'***"
        };

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        private IArkContext _context;
        private IConfig _config;

        public FindTameCommand(IConfig config, IArkContext context)
        {
            _context = context;
            _config = config;
        }

        public void Register(CommandBuilder command)
        {
            command.Parameter("name", ParameterType.Required)
                .Parameter("optional", ParameterType.Multiple);
        }

        public void Init(Discord.DiscordClient client) { }

        public async Task Run(CommandEventArgs e)
        {
            var take = 10;

            var args = CommandHelper.ParseArgs(e, new { Query = "", Exact = false, Species = false, Tribe = "", Owner = "", Skip = 0, OldMap = false }, x =>
                x.For(y => y.Query, noPrefix: true, untilNextToken: true, isRequired: true)
                .For(y => y.Exact, flag: true)
                .For(y => y.Species, flag: true)
                .For(y => y.Tribe, untilNextToken: true)
                .For(y => y.Owner, untilNextToken: true)
                .For(y => y.Skip, defaultValue: 0)
                .For(y => y.OldMap, flag: true));
            if (args == null || args.Skip < 0 || string.IsNullOrWhiteSpace(args.Query) || args.Query.Length < 2)
            {
                await e.Channel.SendMessage(string.Join(Environment.NewLine, new string[] {
                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
                    !string.IsNullOrWhiteSpace(SyntaxHelp) ? $"Help me by following this syntax: **!{Name}** {SyntaxHelp}" : null }.Where(x => x != null)));
                return;
            }

            var aliases = _context.SpeciesAliases?.GetAliases(args.Query);
            var speciesNames = aliases ?? new[] { args.Query };

            var filtered = _context.Creatures?.Where(x => x.Tamed == true);

            if (args.Tribe != null) filtered = filtered.Where(x => x.Tribe != null && x.Tribe.Equals(args.Tribe, StringComparison.OrdinalIgnoreCase));
            if (args.Owner != null)
            {
                //this is what the user expect when they issue the command
                //if they are in a tribe they want the dinos from the tribe
                var tribes = _context.Tribes.Where(x => x.Members != null && x.Members.Contains(args.Owner, StringComparer.OrdinalIgnoreCase)).Select(x => x.Name).ToArray();
                filtered = filtered.Where(x => (x.OwnerName != null && x.OwnerName.Equals(args.Owner, StringComparison.OrdinalIgnoreCase))
                    || (x.Tribe != null && tribes.Length > 0 && tribes.Contains(x.Tribe)));
            }

            if (args.Exact) filtered = filtered?.Where(x => x.Name != null && x.Name.Equals(args.Query, StringComparison.OrdinalIgnoreCase));
            else if (args.Species) filtered = filtered?.Where(x => speciesNames != null && x.SpeciesClass != null && speciesNames.Contains(x.SpeciesClass, StringComparer.OrdinalIgnoreCase));
            else filtered = filtered?.Where(x => (x.Name != null && x.Name.IndexOf(args.Query, StringComparison.OrdinalIgnoreCase) != -1)
                || (speciesNames != null && x.SpeciesClass != null && speciesNames.Contains(x.SpeciesClass, StringComparer.OrdinalIgnoreCase)));

            var matches = filtered?.OrderByDescending(x => x.FullLevel ?? x.BaseLevel).ThenByDescending(x => x.Experience ?? decimal.MinValue).Skip(args.Skip).Take(take).ToArray();
            var count = filtered.Count();
            var nextUpdate = _context.ApproxTimeUntilNextUpdate;
            var nextUpdateTmp = nextUpdate?.ToStringCustom();
            var nextUpdateString = (nextUpdate.HasValue ? (!string.IsNullOrWhiteSpace(nextUpdateTmp) ? $", next update in ~{nextUpdateTmp}" : ", waiting for new update ...") : "");
            var lastUpdate = _context.LastUpdate;
            var lastUpdateString = lastUpdate.ToStringWithRelativeDay();

            if (nextUpdate.HasValue) nextUpdate = TimeSpan.FromSeconds(Math.Round(nextUpdate.Value.TotalSeconds));
            if (matches == null || matches.Length < 1)
            {
                await e.Channel.SendMessage($"**No matching tamed creatures found!** (updated {lastUpdateString}{nextUpdateString})");
                if (args.Species && aliases == null && _context.Creatures != null && _context.SpeciesAliases != null && _context.ArkSpeciesStatsData?.SpeciesStats != null)
                {
                    //var allspecies = _context.Creatures.Select(x => x.SpeciesName).Distinct(StringComparer.OrdinalIgnoreCase).Where(x => !x.Equals("raft", StringComparison.OrdinalIgnoreCase)).ToArray();
                    var sequence = args.Query.ToLower().ToCharArray();
                    var tamableSpecies = _context.ArkSpeciesStatsData.SpeciesStats.Select(x => x.Name).ToArray();
                    //intersection to remove all non-tamable creatures from the list of suggestions (ex. alphas, bosses)
                    var similarity = _context.SpeciesAliases.Aliases.Where(x => tamableSpecies.Intersect(x, StringComparer.OrdinalIgnoreCase).Count() > 0).Select(x =>
                    {
                        var s = x.Select(y => new { key = y, s = StatisticsHelper.CompareToCharacterSequence(y, sequence) }).OrderByDescending(y => y.s).FirstOrDefault();
                        return new { key = s.key, primary = x.FirstOrDefault(), all = x, val = s.s /*s >= 0 ? s : 0*/ };
                    }).ToArray();
                    var possible = StatisticsHelper.FilterUsingStandardDeviation(similarity, x => x.val, (dist, sd) => dist >= sd * 1.5, false);
                    if (possible != null && possible.Length > 0)
                    {
                        var distances = possible.Select((x, i) => new { key = x.key, primary = x.primary, index = i, similarity = x.val, result = args.Query.FindLowestLevenshteinWordDistanceInString(x.key) })
                            .Where(x => x.result != null)
                            .OrderBy(x => x.result.Item2).ThenBy(x => x.similarity).ToArray();
                        var best = StatisticsHelper.FilterUsingStandardDeviation(distances, x => x.result.Item2, (dist, sd) => dist <= sd, false);

                        var suggestions = best.Select(x => $"***\"{x.primary}\"***").ToArray().Join((n, l) => n == l ? " *or* " : "\u200B*,* ");
                        await e.Channel.SendMessage($"*Did you perhaps mean* {suggestions}\u200B*?*"); //\u200B
                    }
                }
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append($"**Found {count} matching tamed creatures");
                if (count > 10) sb.Append(" (showing top " + (args.Skip == 0 ? matches.Length.ToString() : $"{args.Skip + 1}-{args.Skip + matches.Length}") + ")");
                sb.AppendLine($"** (updated {lastUpdateString}{nextUpdateString})");
                foreach (var x in matches)
                {
                    sb.Append($"● {(!string.IsNullOrWhiteSpace(x.Name) ? $"**{x.Name}**, ***{x.SpeciesName}***" : $"**{x.SpeciesName}**")} (lvl ***{x.FullLevel ?? x.BaseLevel}***");
                    if (x.Tribe != null || x.OwnerName != null) sb.Append($" owned by ***{string.Join("/", new[] { x.Tribe, x.OwnerName }.Where(y => !string.IsNullOrWhiteSpace(y)).ToArray())}***");
                    sb.AppendLine(Invariant($") at ***{x.Latitude:N1}***, ***{x.Longitude:N1}***"));
                }

                await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
                await CommandHelper.SendAnnotatedMap(e.Channel, matches.Select(x => new PointF((float)x.Longitude, (float)x.Latitude)).ToArray(), _config.TempFileOutputDirPath, !args.OldMap ? 10f : 5f, template: !args.OldMap ? MapTemplate.Vectorized : MapTemplate.Sketch);
            }
        }
    }
}
