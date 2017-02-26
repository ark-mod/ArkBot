using ArkBot.Data;
using ArkBot.Database;
using ArkBot.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data.Entity.Migrations;
using ArkBot.Extensions;

namespace ArkBot
{
    public class ArkContext : IArkContext
    {
        private ArkSaveFileWatcher _watcher;
        private IConfig _config;
        private IConstants _constants;
        private DatabaseContextFactory<IEfDatabaseContext> _databaseContextFactory;
        public IProgress<string> Progress { get; private set; }

        private string _jsonOutputDirPathTamed => _config?.JsonOutputDirPath != null ? Path.Combine(_config.JsonOutputDirPath, "tamed") : null;
        private string _jsonOutputDirPathWild => _config?.JsonOutputDirPath != null ? Path.Combine(_config.JsonOutputDirPath, "wild") : null;
        private string _jsonOutputDirPathTribes => _config?.JsonOutputDirPath != null ? Path.Combine(_config.JsonOutputDirPath, "tribes") : null;
        private string _jsonOutputDirPathPlayers => _config?.JsonOutputDirPath != null ? Path.Combine(_config.JsonOutputDirPath, "players") : null;
        private string _jsonOutputDirPathCluster => _config?.JsonOutputDirPath != null ? Path.Combine(_config.JsonOutputDirPath, "cluster") : null;

        private const string _speciesstatsFileName = @"arkbreedingstats-values.json";

        private List<DateTime> _previousUpdates = new List<DateTime>();

        private DateTime _lastUpdate;
        public DateTime LastUpdate
        {
            get
            {
                return _lastUpdate;
            }

            private set
            {
                _previousUpdates.Add(value);
                if (_previousUpdates.Count > 20) _previousUpdates.RemoveRange(0, _previousUpdates.Count - 20);

                _lastUpdate = value;
            }
        }

        public TimeSpan? ApproxTimeUntilNextUpdate
        {
            get
            {
                if (_previousUpdates.Count < 2) return null;

                var deltas = _previousUpdates.Skip(1).Zip(_previousUpdates, (a, b) => a - b)
                    .Where(x => x.TotalMilliseconds > 0).ToArray();
                if (deltas.Length <= 0) return null;
                else if (deltas.Length == 1) return deltas[0];

                var avg = deltas.Average(x => x.TotalMilliseconds);
                var sumsd = deltas.Sum(val => (val.TotalMilliseconds - avg) * (val.TotalMilliseconds - avg));
                var sd = Math.Sqrt(sumsd / (deltas.Length - 1));

                var partial = deltas.Where(x => Math.Abs(avg - x.TotalMilliseconds) <= sd).ToArray();
                var estimated = TimeSpan.FromMilliseconds((partial.Length > 0 ? partial : deltas).Average(x => x.TotalMilliseconds));
                var relative = estimated - (DateTime.Now - LastUpdate);

                return TimeSpan.FromMinutes(Math.Round(relative.TotalMinutes));
            }
        }
        public ArkSpeciesAliases SpeciesAliases { get; set; }
        public ArkSpeciesStatsData ArkSpeciesStatsData { get; set; }
        public Creature[] Creatures { get; private set; }
        public Creature[] Wild { get; private set; }
        public Tribe[] Tribes { get; private set; }
        public Player[] Players { get; private set; }
        public CreatureClass[] Classes { get; private set; }
        public Cluster Cluster { get; private set; }
        public List<TribeLog> TribeLogs { get; set; }

        public ArkContext(IConfig config, IConstants constants, IProgress<string> progress, DatabaseContextFactory<IEfDatabaseContext> databaseContextFactory)
        {
            _config = config;
            _constants = constants;
            _databaseContextFactory = databaseContextFactory;
            Progress = progress;
        }

        public async Task Initialize(ArkSpeciesAliases aliases = null)
        {
            Progress.Report("Context initialization started...");

            SpeciesAliases = aliases ?? await ArkSpeciesAliases.Load();

            TribeLogs = new List<TribeLog>();
            var data = await ExtractAndLoadData();
            if (data != null)
            {
                ArkSpeciesStatsData = data.Item1;
                Classes = data.Item2;
                Creatures = data.Item3;
                Players = data.Item4;
                Tribes = data.Item5;
                Cluster = data.Item6;
                Wild = data.Item7;
                _lastUpdate = DateTime.Now; //set backing field in order to not trigger property logic, which would have added the "initialization" time to the last updated collection

                //LogWildCreatures(); //skip doing this here in order to only log events that happen while the bot is running
                //LogTamedCreaturesNew();
            }

            if (!_config.DebugNoExtract)
            {
                _watcher = new ArkSaveFileWatcher { SaveFilePath = _config.SaveFilePath };
                _watcher.Changed += _watcher_Changed;
            }
        }

        private async Task<Tuple<ArkSpeciesStatsData, CreatureClass[], Creature[], Player[], Tribe[], Cluster, WildCreature[]>> ExtractAndLoadData()
        {
            //extract the save file data to json using ark-tools
            Progress.Report("Extracting ARK gamedata...");
            if (!_config.DebugNoExtract && !await ExtractSaveFileData()) return null;

            //load the resulting json into memory
            Progress.Report("Loading json data into memory...");
            var data = await LoadDataFromJson();

            return data;
        }

        private async Task<bool> ExtractSaveFileData()
        {
            try
            {
                await DownloadHelper.DownloadLatestReleaseFromGithub(
                    @"http://api.github.com/repos/Qowyn/ark-tools/releases/latest",
                    (s) => s.EndsWith(".zip", StringComparison.OrdinalIgnoreCase),
                    @"Tools\ark-tools\ark-tools.zip",
                    (p) => p.Equals("ark_data.json", StringComparison.OrdinalIgnoreCase) ? @"Tools\ark-tools\ark_data.json" : null,
                    true,
                    TimeSpan.FromDays(1)
                );
            }
            catch { /*ignore exceptions */ }

            try
            {
                //this resource contains species stats that we need
                await DownloadHelper.DownloadFile(
                    @"https://raw.githubusercontent.com/cadon/ARKStatsExtractor/master/ARKBreedingStats/values.json",
                    _speciesstatsFileName,
                    true,
                    TimeSpan.FromDays(1)
                );
            }
            catch { /*ignore exceptions */ }

            var _rJson = new Regex(@"\.json$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var _rCluster = new Regex(@"^\d+$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var success = true;
            foreach (var action in new[]
            {
                new { inpath = _config.SaveFilePath, path = _jsonOutputDirPathTamed, filepathCheck = _rJson, verb = "tamed", parameters = "" }, //--pretty-print
                new { inpath = _config.SaveFilePath, path = _jsonOutputDirPathWild, filepathCheck = _rJson, verb = "wild", parameters = "" },
                new { inpath = _config.SaveFilePath, path = _jsonOutputDirPathTribes, filepathCheck = _rJson, verb = "tribes", parameters = "--structures --items --tribeless" },
                new { inpath = _config.SaveFilePath, path = _jsonOutputDirPathPlayers, filepathCheck = _rJson, verb = "players", parameters = "--inventory --positions --no-privacy --max-age 2592000" }, // --max-age 2592000 //limit to last 30 days
                new { inpath = _config.ClusterSavePath, path = _jsonOutputDirPathCluster, filepathCheck = _rCluster, verb = "cluster", parameters = "" }
            })
            {
                Progress.Report($"- {action.verb}");

                if (!Directory.Exists(action.path)) Directory.CreateDirectory(action.path);

                try
                {
                    foreach (var filepath in Directory.GetFiles(action.path, "*.*", SearchOption.TopDirectoryOnly))
                    {
                        if (!action.filepathCheck.IsMatch(filepath)) continue;
                        File.Delete(filepath);
                    }

                    var tcs = new TaskCompletionSource<int>();
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Verb = "runas",
                        Arguments = $@"/C {Path.GetFullPath(_config.ArktoolsExecutablePath)} {action.verb} ""{action.inpath}"" ""{action.path}"" {action.parameters}",
                        WorkingDirectory = Directory.GetParent(_config.ArktoolsExecutablePath).FullName,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    var cmd = new Process
                    {
                        StartInfo = startInfo,
                        EnableRaisingEvents = true
                    };
                    cmd.Exited += (sender, args) =>
                    {
                        tcs.SetResult(cmd.ExitCode);
                        cmd.Dispose();
                        cmd = null;
                    };

                    cmd.Start();

                    var exitCode = await tcs.Task;
                    success = success && (exitCode == 0);
                }
                catch
                {
                    /* ignore all exceptions */
                    success = false;
                }
            }

            return success;
        }

        private async Task<Tuple<ArkSpeciesStatsData, CreatureClass[], Creature[], Player[], Tribe[], Cluster, WildCreature[]>> LoadDataFromJson()
        {
            try
            {
                var results = new List<Tuple<string, object>>();

                var actions = new[]
                {
                    new
                    {
                        path = _jsonOutputDirPathTamed,
                        selector = (Func<string, Type>)((x) => x.Equals("classes", StringComparison.OrdinalIgnoreCase) ? typeof(CreatureClass[]) : typeof(TamedCreature[]))
                    },
                    new
                    {
                        path = _jsonOutputDirPathWild,
                        selector = (Func<string, Type>)((x) => x.Equals("classes", StringComparison.OrdinalIgnoreCase) ? typeof(CreatureClass[]) : typeof(WildCreature[]))
                    },
                    new { path = _jsonOutputDirPathTribes, selector = (Func<string, Type>)((x) => typeof(Tribe)) },
                    new { path = _jsonOutputDirPathPlayers, selector = (Func<string, Type>)((x) => typeof(Player)) },
                    new { path = _jsonOutputDirPathCluster, selector = (Func<string, Type>)((x) => typeof(Cluster)) }
                };

                foreach (var action in actions)
                {
                    foreach (var filepath in Directory.EnumerateFiles(action.path, "*.json", SearchOption.TopDirectoryOnly))
                    {
                        var className = Path.GetFileNameWithoutExtension(filepath);

                        using (var reader = File.OpenText(filepath))
                        {
                            var tmp = JsonConvert.DeserializeObject(await reader.ReadToEndAsync(), action.selector(Path.GetFileNameWithoutExtension(filepath)));
                            results.Add(new Tuple<string, object>(className, tmp));
                        }
                    }
                }

                ArkSpeciesStatsData arkSpeciesStatsData = null;
                if (File.Exists(_speciesstatsFileName))
                {
                    using (var reader = File.OpenText(_speciesstatsFileName))
                    {
                        arkSpeciesStatsData = JsonConvert.DeserializeObject<ArkSpeciesStatsData>(await reader.ReadToEndAsync());
                    }
                }
                var classes = results?.Where(x => x.Item2 is CreatureClass[])?.FirstOrDefault()?.Item2 as CreatureClass[];
                var creatures = results?.Where(x => x.Item2 is TamedCreature[])
                    .GroupBy(x => x.Item1)
                    .ToDictionary(x => x.Key, x => x.SelectMany(y => y.Item2 as TamedCreature[]).Select(y =>
                    {
                        y.SpeciesClass = x.Key;
                        y.SpeciesName = classes?.FirstOrDefault(z => z.Class.Equals(x.Key, StringComparison.OrdinalIgnoreCase))?.Name?.Replace("_Character_BP_C", "");

                        return y;
                    }).ToList()).Values.SelectMany(x => x).ToArray();

                var wildcreatures = results?.Where(x => x.Item2 is WildCreature[])
                    .GroupBy(x => x.Item1)
                    .ToDictionary(x => x.Key, x => x.SelectMany(y => y.Item2 as WildCreature[]).Select(y =>
                    {
                        y.SpeciesClass = x.Key;
                        y.SpeciesName = classes?.FirstOrDefault(z => z.Class.Equals(x.Key, StringComparison.OrdinalIgnoreCase))?.Name?.Replace("_Character_BP_C", "");

                        return y;
                    }).ToList()).Values.SelectMany(x => x).ToArray();

                var tribes = results?.Where(x => x.Item2 is Tribe)?.Select(x =>
                {
                    var tribe = x.Item2 as Tribe;
                    int tribeId = 0;
                    if(int.TryParse(x.Item1, out tribeId)) tribe.Id = tribeId;

                    ParseTribeLogs(tribe.TribeLog, tribe.Id ?? -1, tribe.Name);
                    tribe.TribeLog = null;

                    return tribe;
                }).ToArray();
                var players = results?.Where(x => x.Item2 is Player)?.Select(x =>
                {
                    var player = x.Item2 as Player;
                    //SteamId
                    if (string.IsNullOrEmpty(player.SteamId)) player.SteamId = x.Item1;
                    return x.Item2 as Player;
                }).ToArray();

                //todo: not sure how playerid is set
                var clusterlist = results?.Where(x => x.Item2 is Cluster)?.Select(x => x.Item2 as Cluster).ToArray();
                var cluster = new Cluster { Creatures = clusterlist?.SelectMany(x => x.Creatures).ToArray() };

                return new Tuple<ArkSpeciesStatsData, CreatureClass[], Creature[], Player[], Tribe[], Cluster, WildCreature[]>(arkSpeciesStatsData, classes, creatures, players, tribes, cluster, wildcreatures);
            }
            catch { /* ignore all exceptions */ }

            return null;
        }

        /// <summary>
        /// Parse kill logs for use in tame history
        /// </summary>
        private void ParseTribeLogs(string[] logs, int tribeId, string tribeName)
        {
            if (logs == null || tribeName == null) return;

            foreach (var log in logs)
            {
                var item = TameWasKilledTribeLog.FromLog(log);
                if (item == null) continue;
                
                item.TribeId = tribeId;
                item.TribeName = tribeName;
                TribeLogs?.Add(item);
            }
        }

        private async void _watcher_Changed(object sender, ArkSaveFileChangedEventArgs e)
        {
            Progress.Report($"Update triggered by watcher at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            TribeLogs = new List<TribeLog>();
            var data = await ExtractAndLoadData();
            if (data != null)
            {
                ArkSpeciesStatsData = data.Item1;
                Classes = data.Item2;
                Creatures = data.Item3;
                Players = data.Item4;
                Tribes = data.Item5;
                Cluster = data.Item6;
                Wild = data.Item7;

                LastUpdate = DateTime.Now;

                LogWildCreatures();
                LogTamedCreaturesNew();
            }
        }

        private void LogWildCreatures()
        {
            using (var context = _databaseContextFactory.Create())
            {
                //add wild creatures to database
                var wild = Wild?.GroupBy(x => x.SpeciesClass).Select(x => new Database.Model.WildCreatureLogEntry { Key = x.Key, Count = x.Count(), Ids = x.Select(y => y.Id).ToArray() }).ToArray();
                if (wild == null || wild.Length <= 0) return;

                var removeBefore = DateTime.Now.AddDays(-7);
                foreach (var item in context.WildCreatureLogs.Where(x => x.When < removeBefore).ToArray())
                {
                    context.WildCreatureLogs.Remove(item);
                }

                context.WildCreatureLogs.Add(new Database.Model.WildCreatureLog { When = LastUpdate, Entries = wild });
                context.SaveChanges();
            }
        }

        private void LogTamedCreaturesNew()
        {
            var st = Stopwatch.StartNew();
            using (var conn = new System.Data.SqlServerCe.SqlCeConnection(System.Configuration.ConfigurationManager.ConnectionStrings["EfDatabaseContext"].ConnectionString))
            {
                conn.Open();

                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        var l = new Database.Model.TamedCreatureLogEntry();

                        //add tamed creatures to database
                        var tamed = Creatures?.OrderBy(x => x.TamedTime).GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToArray();
                        if (tamed == null || tamed.Length == 0) return;

                        using (var command = conn.CreateCommand())
                        {
                            command.CommandText = "SELECT * FROM TamedCreatureLogEntries";
                            command.CommandType = System.Data.CommandType.Text;

                            using (var resultSet = command.ExecuteResultSet(System.Data.SqlServerCe.ResultSetOptions.Scrollable | System.Data.SqlServerCe.ResultSetOptions.Updatable))
                            {
                                var ordinal = new Func<string, int>(x => resultSet.GetOrdinal(x));

                                var getValues = new Func<Creature, object[]>(tame =>
                                {
                                    var maxFood = CalculateMaxFood(tame.SpeciesClass ?? tame.SpeciesName, tame.WildLevels?.Food, tame.TamedLevels?.Food, tame.ImprintingQuality);
                                    var approxFoodPercentage = tame.CurrentFood.HasValue && maxFood.HasValue ? (double)tame.CurrentFood.Value / maxFood.Value : (double?)null;
                                    return new[] {
                                        new { ordinal = ordinal(nameof(l.Id)), value = (object)tame.Id },
                                        new { ordinal = ordinal(nameof(l.LastSeen)), value = (object)LastUpdate },
                                        new { ordinal = ordinal(nameof(l.Latitude)), value = (object)tame.Latitude },
                                        new { ordinal = ordinal(nameof(l.Longitude)), value = (object)tame.Longitude },
                                        new { ordinal = ordinal(nameof(l.X)), value = (object)tame.X },
                                        new { ordinal = ordinal(nameof(l.Y)), value = (object)tame.Y },
                                        new { ordinal = ordinal(nameof(l.Z)), value = (object)tame.Z },
                                        new { ordinal = ordinal(nameof(l.Name)), value = (object)tame.Name },
                                        new { ordinal = ordinal(nameof(l.Team)), value = (object)tame.Team },
                                        new { ordinal = ordinal(nameof(l.PlayerId)), value = (object)tame.PlayerId },
                                        new { ordinal = ordinal(nameof(l.Tribe)), value = (object)tame.Tribe },
                                        new { ordinal = ordinal(nameof(l.OwnerName)), value = (object)tame.OwnerName },
                                        new { ordinal = ordinal(nameof(l.Tamer)), value = (object)tame.Tamer },
                                        new { ordinal = ordinal(nameof(l.Female)), value = (object)tame.Female },
                                        new { ordinal = ordinal(nameof(l.BaseLevel)), value = (object)tame.BaseLevel },
                                        new { ordinal = ordinal(nameof(l.FullLevel)), value = (object)tame.FullLevel },
                                        new { ordinal = ordinal(nameof(l.Experience)), value = (object)tame.Experience },
                                        new { ordinal = ordinal(nameof(l.ApproxFoodPercentage)), value = (object)approxFoodPercentage },
                                        new { ordinal = ordinal(nameof(l.ImprintingQuality)), value = (object)tame.ImprintingQuality },
                                        new { ordinal = ordinal(nameof(l.TamedAtTime)), value = (object)tame.TamedAtTime },
                                        new { ordinal = ordinal(nameof(l.TamedTime)), value = (object)tame.TamedTime },
                                        new { ordinal = ordinal(nameof(l.RelatedLogEntries)), value = (object)null },
                                        new { ordinal = ordinal(nameof(l.SpeciesClass)), value = (object)tame.SpeciesClass },
                                        new { ordinal = ordinal(nameof(l.IsConfirmedDead)), value = (object)false },
                                        new { ordinal = ordinal(nameof(l.IsInCluster)), value = (object)false },
                                        new { ordinal = ordinal(nameof(l.IsUnavailable)), value = (object)false }
                                        }.OrderBy(x => x.ordinal).Select(x => x.value).ToArray();
                                });

                                var ids = tamed.Select(x => x.Id).ToList();
                                if (resultSet.HasRows)
                                {
                                    resultSet.ReadFirst();

                                    do
                                    {
                                        var id = resultSet.SafeGet<long>(nameof(l.Id));
                                        var tame = tamed.FirstOrDefault(x => x.Id == id);
                                        if (tame == null)
                                        {
                                            //this tame is now dead or missing we should update the state
                                            var name = resultSet.SafeGet<string>(nameof(l.Name));
                                            var team = resultSet.SafeGet<int?>(nameof(l.Team));
                                            var speciesClass = resultSet.SafeGet<string>(nameof(l.SpeciesClass));
                                            var fullLevel = resultSet.SafeGet<int?>(nameof(l.FullLevel));
                                            var baseLevel = resultSet.SafeGet<int>(nameof(l.BaseLevel));

                                            var tst = team.HasValue && name != null && speciesClass != null ? TribeLogs?.OfType<TameWasKilledTribeLog>()
                                                .Where(x =>
                                                        x.TribeId == team.Value).ToArray() : null;

                                            var relatedLogs = team.HasValue && name != null && speciesClass != null ? TribeLogs?.OfType<TameWasKilledTribeLog>()
                                                .Where(x =>
                                                        x.TribeId == team.Value
                                                        && name.Equals(x.Name, StringComparison.Ordinal)
                                                        && x.Level == (fullLevel ?? baseLevel)
                                                        && ((SpeciesAliases.GetAliases(x.SpeciesName)?.Contains(speciesClass)) ?? false)).ToArray() : null;

                                            //is confirmed dead
                                            var isConfirmedDead = resultSet.SafeGet<bool>(nameof(l.IsConfirmedDead));
                                            isConfirmedDead = isConfirmedDead || (relatedLogs != null && relatedLogs.Length > 0);
                                            resultSet.SetBoolean(ordinal(nameof(l.IsConfirmedDead)), isConfirmedDead);

                                            //is in cluster
                                            var isInCluster = Cluster?.Creatures.Any(x => x.Id == id) ?? false;
                                            resultSet.SetBoolean(ordinal(nameof(l.IsInCluster)), isInCluster);

                                            //is unavailable
                                            resultSet.SetBoolean(ordinal(nameof(l.IsUnavailable)), true);

                                            //related logs
                                            if (relatedLogs != null && relatedLogs.Length > 0)
                                            {
                                                var oldRelatedLogs = resultSet.SafeGet<string>(nameof(l.RelatedLogEntries));
                                                var newRelatedLogs = relatedLogs.Select(x => x.Message).ToArray();
                                                if (oldRelatedLogs != null) newRelatedLogs = oldRelatedLogs.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Concat(newRelatedLogs).Distinct().ToArray();
                                                resultSet.SetString(ordinal(nameof(l.RelatedLogEntries)), string.Join(Environment.NewLine, newRelatedLogs));
                                            }

                                            resultSet.Update();

                                            continue;
                                        }

                                        ids.Remove(id);
                                        resultSet.SetValues(getValues(tame).Select(x => x == null ? DBNull.Value : x).ToArray());
                                        resultSet.Update();
                                    } while (resultSet.Read());
                                }

                                foreach (var id in ids)
                                {
                                    var tame = tamed.FirstOrDefault(x => x.Id == id);
                                    if (tame == null) continue;

                                    var record = resultSet.CreateRecord();
                                    record.SetValues(getValues(tame).Select(x => x == null ? DBNull.Value : x).ToArray());

                                    resultSet.Insert(record, System.Data.SqlServerCe.DbInsertOptions.PositionOnInsertedRow);
                                }
                            }
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            Debug.WriteLine($"{nameof(LogTamedCreaturesNew)} finished in {st.ElapsedMilliseconds:N0} ms");
        }

        //sqlcompact have crap performance, no support for clustered indexes, attaching entities takes a milion years
        //this code is the fastest way to achieve this using ef (note the query to get all entities from database rather than specific ids)
        //private void LogTamedCreatures()
        //{
        //    var st = Stopwatch.StartNew();
        //    using (var context = _databaseContextFactory.Create())
        //    {
        //        //add tamed creatures to database
        //        var tamed = Creatures?.OrderBy(x => x.TamedTime).GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToArray();
        //        var ids = tamed?.Select(x => x.Id).ToArray();
        //        if (ids == null || ids.Length == 0) return;

        //        //var duplicateIds = ids.GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.Key).ToArray();
        //        //var duplicateTames = duplicateIds.Select(x => tamed.Where(y => y.Id == x).ToArray()).ToArray();

        //        var st2 = Stopwatch.StartNew();
        //        var fromDatabase = context.TamedCreatureLogEntries.ToArray(); //.Where(x => ids.Contains(x.Id)).ToArray();
        //        //var idsFromDatabase = context.TamedCreatureLogEntries.Select(x => x.Id).ToArray();
        //        Debug.WriteLine($"{nameof(LogTamedCreatures)} get all from database finished in {st2.ElapsedMilliseconds:N0} ms");

        //        var st3 = Stopwatch.StartNew();
        //        //context.TamedCreatureLogEntries.AddOrUpdate(
        //        //    x => x.Id,
        //        //    tamed.Select(tame => new Database.Model.TamedCreatureLogEntry
        //        //    {
        //        //        Id = tame.Id,
        //        //        LastSeen = LastUpdate,
        //        //        Latitude = tame.Latitude,
        //        //        Longitude = tame.Longitude,
        //        //        X = tame.X,
        //        //        Y = tame.Y,
        //        //        Z = tame.Z,
        //        //        Name = tame.Name,
        //        //        Team = tame.Team,
        //        //        PlayerId = tame.PlayerId,
        //        //        Tribe = tame.Tribe,
        //        //        OwnerName = tame.OwnerName,
        //        //        Tamer = tame.Tamer,
        //        //        Female = tame.Female,
        //        //        BaseLevel = tame.BaseLevel,
        //        //        FullLevel = tame.FullLevel,
        //        //        Experience = tame.Experience,
        //        //        CurrentFood = tame.CurrentFood,
        //        //        ImprintingQuality = tame.ImprintingQuality,
        //        //        TamedAtTime = tame.TamedAtTime,
        //        //        TamedTime = tame.TamedTime,
        //        //        RelatedLogEntry = null //todo: fix
        //        //    }).ToArray());

        //        foreach (var id in ids)
        //        {
        //            var tame = tamed.FirstOrDefault(x => x.Id == id);
        //            if (tame == null) continue;

        //            //var dbo = new Database.Model.TamedCreatureLogEntry
        //            //{
        //            //    Id = tame.Id,
        //            //    LastSeen = LastUpdate,
        //            //    Latitude = tame.Latitude,
        //            //    Longitude = tame.Longitude,
        //            //    X = tame.X,
        //            //    Y = tame.Y,
        //            //    Z = tame.Z,
        //            //    Name = tame.Name,
        //            //    Team = tame.Team,
        //            //    PlayerId = tame.PlayerId,
        //            //    Tribe = tame.Tribe,
        //            //    OwnerName = tame.OwnerName,
        //            //    Tamer = tame.Tamer,
        //            //    Female = tame.Female,
        //            //    BaseLevel = tame.BaseLevel,
        //            //    FullLevel = tame.FullLevel,
        //            //    Experience = tame.Experience,
        //            //    CurrentFood = tame.CurrentFood,
        //            //    ImprintingQuality = tame.ImprintingQuality,
        //            //    TamedAtTime = tame.TamedAtTime,
        //            //    TamedTime = tame.TamedTime,
        //            //    RelatedLogEntry = null //todo: fix
        //            //};

        //            //if (idsFromDatabase.Any(x => x == id))
        //            //{
        //            //    context.TamedCreatureLogEntries.Attach(dbo);
        //            //    context.Entry(dbo).State = System.Data.Entity.EntityState.Modified;
        //            //}
        //            //else
        //            //{
        //            //    context.TamedCreatureLogEntries.Add(dbo);
        //            //}

        //            //var dbo = context.TamedCreatureLogEntries.SingleOrDefault(x => x.Id == id);
        //            var dbo = fromDatabase.FirstOrDefault(x => x.Id == id);
        //            if (dbo == null)
        //            {
        //                //does seem to be seconds from when the server was created rather than a full date
        //                //var tamedAt = (DateTime?)null;
        //                //if (tame.TamedAtTime.HasValue)
        //                //{
        //                //    tamedAt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds((double)tame.TamedAtTime.Value);
        //                //}

        //                //not yet logged in database
        //                context.TamedCreatureLogEntries.Add(new Database.Model.TamedCreatureLogEntry
        //                {
        //                    Id = tame.Id,
        //                    LastSeen = LastUpdate,
        //                    Latitude = tame.Latitude,
        //                    Longitude = tame.Longitude,
        //                    X = tame.X,
        //                    Y = tame.Y,
        //                    Z = tame.Z,
        //                    Name = tame.Name,
        //                    Team = tame.Team,
        //                    PlayerId = tame.PlayerId,
        //                    Tribe = tame.Tribe,
        //                    OwnerName = tame.OwnerName,
        //                    Tamer = tame.Tamer,
        //                    Female = tame.Female,
        //                    BaseLevel = tame.BaseLevel,
        //                    FullLevel = tame.FullLevel,
        //                    Experience = tame.Experience,
        //                    CurrentFood = tame.CurrentFood,
        //                    ImprintingQuality = tame.ImprintingQuality,
        //                    TamedAtTime = tame.TamedAtTime,
        //                    TamedTime = tame.TamedTime,
        //                    RelatedLogEntry = null //todo: fix
        //                });
        //            }
        //            else
        //            {
        //                //if required, update with new information
        //                dbo.LastSeen = LastUpdate;
        //                dbo.Latitude = tame.Latitude;
        //                dbo.Longitude = tame.Longitude;
        //                dbo.X = tame.X;
        //                dbo.Y = tame.Y;
        //                dbo.Z = tame.Z;
        //                dbo.Name = tame.Name;
        //                dbo.Team = tame.Team;
        //                dbo.PlayerId = tame.PlayerId;
        //                dbo.Tribe = tame.Tribe;
        //                dbo.OwnerName = tame.OwnerName;
        //                dbo.Tamer = tame.Tamer;
        //                dbo.Female = tame.Female;
        //                dbo.BaseLevel = tame.BaseLevel;
        //                dbo.FullLevel = tame.FullLevel;
        //                dbo.Experience = tame.Experience;
        //                dbo.CurrentFood = tame.CurrentFood;
        //                dbo.ImprintingQuality = tame.ImprintingQuality;
        //                dbo.TamedAtTime = tame.TamedAtTime;
        //                dbo.TamedTime = tame.TamedTime;
        //                dbo.RelatedLogEntry = null; //todo: fix
        //            }
        //        }
        //        Debug.WriteLine($"{nameof(LogTamedCreatures)} add/update finished in {st3.ElapsedMilliseconds:N0} ms");
        //        var st4 = Stopwatch.StartNew();
        //        context.SaveChanges();
        //        Debug.WriteLine($"{nameof(LogTamedCreatures)} save finished in {st4.ElapsedMilliseconds:N0} ms");
        //    }
        //    st.Stop();
        //    Debug.WriteLine($"{nameof(LogTamedCreatures)} finished in {st.ElapsedMilliseconds:N0} ms");
        //}

        public double? CalculateMaxFood(string speciesNameOrClass, int? wildLevelFood, int? tamedLevelFood, decimal? imprintingQuality)
        {
            var speciesAliases = SpeciesAliases?.GetAliases(speciesNameOrClass) ?? new[] { speciesNameOrClass };
            return ArkSpeciesStatsData?.GetMaxValue(
                            speciesAliases, //a list of alternative species names
                            Data.ArkSpeciesStatsData.Stat.Food,
                            wildLevelFood ?? 0,
                            tamedLevelFood ?? 0,
                            1d, //todo: taming efficiency is missing from ark-tools (?)
                            (double)(imprintingQuality ?? 0m));
        }

        public string GetElevationAsText(decimal z)
        {
            //for the island
            //550	Map ceiling (insta-death)
            //460	Volcano's Peak
            //0	Nothing special
            //-145	Sea level
            //-285	Lowest(?) point on land
            //-460	Sea floor
            //-480	Map floor (insta-death)

            var elevation = (double)z / 100d;
            var amsl = elevation + 145;
            var hint = elevation <= -480 ? "below world" : elevation <= -450 ? "sea floor" : elevation <= -285 ? "underground" : elevation >= -165 && elevation <= -125 ? "sea level" : elevation >= 550 ? "above world" : elevation >= 525 ? "map ceiling" : null;

            return $"{amsl:N0} meters" + (hint != null ? $" (~{hint})" : "");
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _watcher?.Dispose();
                    _watcher = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ArkContext() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
