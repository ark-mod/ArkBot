//using ArkBot.Data;
//using ArkBot.Database;
//using ArkBot.Helpers;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Data.Entity.Core.Objects;
//using System.Diagnostics;
//using System.IO;
//using System.IO.Compression;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using System.Data.Entity.Migrations;
//using ArkBot.Extensions;
//using ArkBot.Services;
//using System.Data.Entity;
//using ArkBot.Database.Model;
//using ArkBot.Services.Data;
//using System.Threading;
//using ArkBot.Threading;
//using System.Transactions;
//using ArkBot.ViewModel;
//using ArkBot.Ark;

//namespace ArkBot
//{
//    public class ArkContext : IArkContext
//    {
//        private IArkSaveFileWatcher _watcher;
//        private IConfig _config;
//        private IConstants _constants;
//        private ISavedState _savedstate;
//        private EfDatabaseContextFactory _databaseContextFactory;
//        private IPlayedTimeWatcher _playedTimeWatcher;
//        private ISavegameBackupService _savegameBackupService;
//        private ArkContextManager _contextManager;

//        public IProgress<string> Progress { get; private set; }
//        private SingleRunningTaskCancelPrevious _contextUpdateSync;

//        public event ContextUpdating Updating;
//        public event ContextUpdated Updated;
//        //public event VoteInitiatedEventHandler VoteInitiated;
//        //public event VoteResultForcedEventHandler VoteResultForced;

//        private const string _jsonSubDirTamed = "tamed";
//        private const string _jsonSubDirWild = "wild";
//        private const string _jsonSubDirTribes = "tribes";
//        private const string _jsonSubDirPlayers = "players";
//        private const string _jsonSubDirCluster = "cluster";

//        private string _jsonOutputDirPathTamed => _config?.JsonOutputDirPath != null ? Path.Combine(_config.JsonOutputDirPath, _jsonSubDirTamed) : null;
//        private string _jsonOutputDirPathWild => _config?.JsonOutputDirPath != null ? Path.Combine(_config.JsonOutputDirPath, _jsonSubDirWild) : null;
//        private string _jsonOutputDirPathTribes => _config?.JsonOutputDirPath != null ? Path.Combine(_config.JsonOutputDirPath, _jsonSubDirTribes) : null;
//        private string _jsonOutputDirPathPlayers => _config?.JsonOutputDirPath != null ? Path.Combine(_config.JsonOutputDirPath, _jsonSubDirPlayers) : null;
//        private string _jsonOutputDirPathCluster => _config?.JsonOutputDirPath != null ? Path.Combine(_config.JsonOutputDirPath, _jsonSubDirCluster) : null;

//        private List<DateTime> _previousUpdates = new List<DateTime>();
//        private TribeLog _latestTribeLog;
//        private bool _contextUpdatesDisabledOverride = false;

//        public bool IsInitialized { get; set; }

//        private DateTime _lastUpdate;
//        public DateTime LastUpdate
//        {
//            get
//            {
//                return _lastUpdate;
//            }

//            private set
//            {
//                _previousUpdates.Add(value);
//                if (_previousUpdates.Count > 20) _previousUpdates.RemoveRange(0, _previousUpdates.Count - 20);

//                _lastUpdate = value;
//            }
//        }

//        public TimeSpan? ApproxTimeUntilNextUpdate
//        {
//            get
//            {
//                if (_previousUpdates.Count < 2) return null;

//                var deltas = _previousUpdates.Skip(1).Zip(_previousUpdates, (a, b) => a - b)
//                    .Where(x => x.TotalMilliseconds > 0).ToArray();
//                if (deltas.Length <= 0) return null;
//                else if (deltas.Length == 1) return deltas[0];

//                var avg = deltas.Average(x => x.TotalMilliseconds);
//                var sumsd = deltas.Sum(val => (val.TotalMilliseconds - avg) * (val.TotalMilliseconds - avg));
//                var sd = Math.Sqrt(sumsd / (deltas.Length - 1));

//                var partial = deltas.Where(x => Math.Abs(avg - x.TotalMilliseconds) <= sd).ToArray();
//                var estimated = TimeSpan.FromMilliseconds((partial.Length > 0 ? partial : deltas).Average(x => x.TotalMilliseconds));
//                var relative = estimated - (DateTime.Now - LastUpdate);

//                return TimeSpan.FromMinutes(Math.Round(relative.TotalMinutes));
//            }
//        }

//        public ArkSpeciesStatsData ArkSpeciesStatsData { get; set; }
//        public Creature[] Creatures { get; private set; }
//        public Creature[] Wild { get; private set; }
//        public Tribe[] Tribes { get; private set; }
//        public Player[] Players { get; private set; }
//        public CreatureClass[] Classes { get; private set; }
//        public Cluster Cluster { get; private set; }
//        public List<TribeLog> TribeLogs { get; set; }

//        public IEnumerable<Creature> CreaturesNoRaft => Creatures?.Where(x => x != null && x.SpeciesClass != null && !x.SpeciesClass.Equals("Raft_BP_C", StringComparison.OrdinalIgnoreCase));
//        public IEnumerable<Creature> CreaturesInclCluster => (Creatures ?? new Creature[] { }).Concat((Cluster?.Creatures ?? new Creature[] { }));
//        public IEnumerable<Creature> CreaturesInclClusterNoRaft => CreaturesInclCluster.Where(x => x != null && x.SpeciesClass != null && !x.SpeciesClass.Equals("Raft_BP_C", StringComparison.OrdinalIgnoreCase));

//        public ArkContext(
//            IConfig config, 
//            IConstants constants, 
//            IProgress<string> progress,
//            ISavedState savedstate,
//            EfDatabaseContextFactory databaseContextFactory,
//            IPlayedTimeWatcher playedTimeWatcher,
//            ISavegameBackupService savegameBackupService,
//            ArkContextManager contextManager)
//        {
//            _config = config;
//            _constants = constants;
//            _savedstate = savedstate;
//            _databaseContextFactory = databaseContextFactory;
//            _playedTimeWatcher = playedTimeWatcher;
//            _savegameBackupService = savegameBackupService;
//            _contextManager = contextManager;
//            Progress = progress;
//            _contextUpdateSync = new SingleRunningTaskCancelPrevious();

//            Creatures = new Creature[] { };
//            Wild = new Creature[] { };
//            Tribes = new Tribe[] { };
//            Players = new Player[] { };
//            Classes = new CreatureClass[] { };
//            Cluster = new Cluster();
//            TribeLogs = new List<TribeLog>();
//            ArkSpeciesStatsData = new ArkSpeciesStatsData();
//        }

//        public async Task Initialize(CancellationToken token, bool skipExtract = false)
//        {
//            Progress.Report("Context initialization started...");

//            //Updating?.Invoke(this, new ContextUpdatingEventArgs(false));

//            TribeLogs = new List<TribeLog>();
//            //var data = await ExtractAndLoadData(token, skipExtract);
//            //if (data != null)
//            //{
//            //    ArkSpeciesStatsData = data.Item1 ?? new ArkSpeciesStatsData();
//            //    Classes = data.Item2 ?? new CreatureClass[] { };
//            //    Creatures = data.Item3 ?? new Creature[] { };
//            //    Players = data.Item4 ?? new Player[] { };
//            //    Tribes = data.Item5 ?? new Tribe[] { };
//            //    Cluster = data.Item6 ?? new Cluster();
//            //    Wild = data.Item7 ?? new Creature[] { };
//            //    _lastUpdate = DateTime.Now; //set backing field in order to not trigger property logic, which would have added the "initialization" time to the last updated collection

//            //    //LogWildCreatures(); //skip doing this here in order to only log events that happen while the bot is running
//            //    LogTamedCreaturesNew(CancellationToken.None);

//            //    Updated?.Invoke(this, EventArgs.Empty);
//            //}


//            //if (_config.Debug)
//            //{
//            //    _watcher = new DebugFakeSaveFileWatcher(_config, TimeSpan.FromMinutes(1));
//            //    _watcher.Changed += _watcher_DebugFakeChanged;
//            //}
//            //else if (!_config.DebugNoExtract)
//            //{
//                _contextManager.UpdateCompleted += ContextManager_UpdateCompleted;
//            //}

//            _playedTimeWatcher.PlayedTimeUpdate += _playedTimeWatcher_PlayedTimeUpdate;
//            _playedTimeWatcher.Start();

//            Progress.Report("Initialization done!" + Environment.NewLine);
//        }

//        private async void ContextManager_UpdateCompleted(Ark.IArkUpdateableContext sender, bool successful, bool cancelled)
//        {
//            var serverContext = sender as Ark.ArkServerContext;
//            if (cancelled == false && serverContext != null && serverContext.Config.Key.Equals(_config.ServerKey) && !_contextUpdatesDisabledOverride)
//            {
//                Progress.Report($"Context: Update triggered by watcher at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
//                //await UpdateAll();
//                if (await _contextUpdateSync.Execute(async (ct) => await UpdateAll(ct)))
//                    Progress.Report($"Context: Update complete!");
//                else
//                    Progress.Report($"Context: Update cancelled/failed.");
//            }
//        }

//        //public void DebugTriggerOnChange()
//        //{
//        //    if (!(_watcher is DebugFakeSaveFileWatcher)) return;

//        //    (_watcher as DebugFakeSaveFileWatcher).OnChanged();
//        //}

//        private void _playedTimeWatcher_PlayedTimeUpdate(object sender, PlayedTimeWatcher.PlayedTimeEventArgs e)
//        {
//            var now = DateTime.Now.Date;
//            var totalSeconds = (int)Math.Round(e.TimeToAdd.TotalSeconds);
//            using (var db = _databaseContextFactory.Create())
//            {
//                foreach(var name in e.Players)
//                {
//                    var player = Players?.FirstOrDefault(x => x.PlayerName.Equals(name, StringComparison.Ordinal));
//                    long steamId;
//                    if (player == null || !long.TryParse(player.SteamId, out steamId)) continue;
                    
//                    var user = db.Users.FirstOrDefault(x => x.SteamId == steamId);
//                    if (user != null)
//                    {
//                        //update user
//                        var today = user.Played.OrderByDescending(x => x.Date).FirstOrDefault(x => x.Date.Date == now);
//                        if (today != null) today.TimeInSeconds += totalSeconds;
//                        else user.Played.Add(new Database.Model.PlayedEntry { Date = now, TimeInSeconds = totalSeconds });
//                    }
//                    else
//                    {
//                        //update using steamid
//                        var today = db.Played.OrderByDescending(x => x.Date).FirstOrDefault(x => x.SteamId == steamId && DbFunctions.TruncateTime(x.Date) == now);
//                        if (today != null) today.TimeInSeconds += totalSeconds;
//                        else db.Played.Add(new Database.Model.PlayedEntry { Date = now, TimeInSeconds = totalSeconds, SteamId = steamId });
//                    }
//                }

//                db.SaveChanges();
//            }
//        }

//        private async Task<Tuple<ArkSpeciesStatsData, CreatureClass[], Creature[], Player[], Tribe[], Cluster, WildCreature[]>> ExtractAndLoadData(CancellationToken ct, bool skipExtract = false, string jsonPathOverride = null, string saveFilePathOverride = null, string clusterPathOverride = null)
//        {
//            ct.ThrowIfCancellationRequested();

//            //extract the save file data to json using ark-tools
//            //if (!(_config.DebugNoExtract || skipExtract))
//            if (!skipExtract)
//            {
//                Progress.Report("Extracting ARK gamedata...");
//                if (!await ExtractSaveFileData(ct, saveFilePathOverride, clusterPathOverride)) return null;
//            }

//            //load the resulting json into memory
//            Progress.Report("Loading json data into memory...");
//            var data = await LoadDataFromJson(ct, jsonPathOverride);

//            return data;
//        }

//        private async Task<bool> ExtractSaveFileData(CancellationToken ct, string saveFilePathOverride = null, string clusterPathOverride = null)
//        {
//            ct.ThrowIfCancellationRequested();
//            try
//            {
//                await DownloadHelper.DownloadLatestReleaseFromGithub(
//                    @"http://api.github.com/repos/Qowyn/ark-tools/releases/latest",
//                    (s) => s.EndsWith(".zip", StringComparison.OrdinalIgnoreCase),
//                    @"Tools\ark-tools\ark-tools.zip",
//                    (p) => p.Equals("ark_data.json", StringComparison.OrdinalIgnoreCase) ? @"Tools\ark-tools\ark_data.json" : null,
//                    true,
//                    TimeSpan.FromDays(1)
//                );
//            }
//            catch { /*ignore exceptions */ }

//            ct.ThrowIfCancellationRequested();
//            try
//            {
//                await ArkSpeciesStats.Instance.LoadOrUpdate();
//            }
//            catch { /*ignore exceptions */ }

//            ct.ThrowIfCancellationRequested();
//            var _rJson = new Regex(@"\.json$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
//            var _rCluster = new Regex(@"^\d+$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

//            //clean all json files in output dir
//            try
//            {
//                if (Directory.Exists(_config.JsonOutputDirPath))
//                {
//                    foreach (var filepath in Directory.GetFiles(_config.JsonOutputDirPath, "*.json", SearchOption.TopDirectoryOnly)) File.Delete(filepath);
//                }
//            } catch { /* ignore all exceptions */ }

//            var success = true;
//            foreach (var action in new[]
//            {
//                new { inpath = saveFilePathOverride ?? _config.SaveFilePath, path = _jsonOutputDirPathTamed, filepathCheck = _rJson, verb = "tamed", parameters = "" }, //--pretty-print
//                new { inpath = saveFilePathOverride ?? _config.SaveFilePath, path = _jsonOutputDirPathWild, filepathCheck = _rJson, verb = "wild", parameters = "" },
//                new { inpath = saveFilePathOverride ?? _config.SaveFilePath, path = _jsonOutputDirPathTribes, filepathCheck = _rJson, verb = "tribes", parameters = "--structures --items --tribeless" },
//                new { inpath = saveFilePathOverride ?? _config.SaveFilePath, path = _jsonOutputDirPathPlayers, filepathCheck = _rJson, verb = "players", parameters = "--inventory --positions --no-privacy --max-age 2592000" }, // --max-age 2592000 //limit to last 30 days
//                new { inpath = clusterPathOverride ?? _config.ClusterSavePath, path = _jsonOutputDirPathCluster, filepathCheck = _rCluster, verb = "cluster", parameters = "" }
//            })
//            {
//                ct.ThrowIfCancellationRequested();
//                Progress.Report($"- {action.verb}");

//                if (!Directory.Exists(action.path)) Directory.CreateDirectory(action.path);

//                Process cmd = null;
//                try
//                {
//                    foreach (var filepath in Directory.GetFiles(action.path, "*.*", SearchOption.TopDirectoryOnly))
//                    {
//                        if (!action.filepathCheck.IsMatch(filepath)) continue;
//                        File.Delete(filepath);
//                    }

//                    var tcs = new TaskCompletionSource<int>();
//                    ct.Register(() => tcs.TrySetCanceled(ct));
//                    var startInfo = new ProcessStartInfo
//                    {
//                        FileName = "cmd.exe",
//                        Verb = "runas",
//                        Arguments = $@"/C {Path.GetFullPath(_config.ArktoolsExecutablePath)} {action.verb} ""{action.inpath}"" ""{action.path}"" {action.parameters}",
//                        WorkingDirectory = Directory.GetParent(_config.ArktoolsExecutablePath).FullName,
//                        CreateNoWindow = true,
//                        WindowStyle = ProcessWindowStyle.Hidden
//                    };
//                    cmd = new Process
//                    {
//                        StartInfo = startInfo,
//                        EnableRaisingEvents = true
//                    };
//                    cmd.Exited += (sender, args) =>
//                    {
//                        tcs.TrySetResult(cmd.ExitCode);
//                    };

//                    cmd.Start();

//                    var exitCode = await tcs.Task;
//                    success = success && (exitCode == 0);
//                }
//                catch (OperationCanceledException) { throw; }
//                catch
//                {
//                    /* ignore all exceptions */
//                    success = false;
//                }
//                finally
//                {
//                    if (cmd != null && !cmd.HasExited) cmd.KillTree();
//                    cmd?.Dispose();
//                    cmd = null;
//                }
//            }

//            return success;
//        }

//        private async Task<Tuple<ArkSpeciesStatsData, CreatureClass[], Creature[], Player[], Tribe[], Cluster, WildCreature[]>> LoadDataFromJson(CancellationToken ct, string jsonPathOverride = null)
//        {
//            ct.ThrowIfCancellationRequested();
//            try
//            {
//                var results = new List<Tuple<string, object>>();

//                var actions = new[]
//                {
//                    new
//                    {
//                        path = jsonPathOverride != null ? Path.Combine(jsonPathOverride, _jsonSubDirTamed) : _jsonOutputDirPathTamed,
//                        selector = (Func<string, Type>)((x) => x.Equals("classes", StringComparison.OrdinalIgnoreCase) ? typeof(CreatureClass[]) : typeof(TamedCreature[]))
//                    },
//                    new
//                    {
//                        path = jsonPathOverride != null ? Path.Combine(jsonPathOverride, _jsonSubDirWild) : _jsonOutputDirPathWild,
//                        selector = (Func<string, Type>)((x) => x.Equals("classes", StringComparison.OrdinalIgnoreCase) ? typeof(CreatureClass[]) : typeof(WildCreature[]))
//                    },
//                    new { path = jsonPathOverride != null ? Path.Combine(jsonPathOverride, _jsonSubDirTribes) : _jsonOutputDirPathTribes, selector = (Func<string, Type>)((x) => typeof(Tribe)) },
//                    new { path = jsonPathOverride != null ? Path.Combine(jsonPathOverride, _jsonSubDirPlayers) : _jsonOutputDirPathPlayers, selector = (Func<string, Type>)((x) => typeof(Player)) },
//                    new { path = jsonPathOverride != null ? Path.Combine(jsonPathOverride, _jsonSubDirCluster) : _jsonOutputDirPathCluster, selector = (Func<string, Type>)((x) => typeof(Cluster)) }
//                };

//                foreach (var action in actions)
//                {
//                    ct.ThrowIfCancellationRequested();

//                    foreach (var filepath in Directory.EnumerateFiles(action.path, "*.json", SearchOption.TopDirectoryOnly))
//                    {
//                        var className = Path.GetFileNameWithoutExtension(filepath);

//                        using (var reader = File.OpenText(filepath))
//                        {
//                            var tmp = JsonConvert.DeserializeObject(await reader.ReadToEndAsync(), action.selector(Path.GetFileNameWithoutExtension(filepath)));
//                            results.Add(new Tuple<string, object>(className, tmp));
//                        }
//                    }
//                }
                
//                var classes = results?.Where(x => x.Item2 is CreatureClass[])?.FirstOrDefault()?.Item2 as CreatureClass[];
//                var creatures = results?.Where(x => x.Item2 is TamedCreature[])
//                    .GroupBy(x => x.Item1)
//                    .ToDictionary(x => x.Key, x => x.SelectMany(y => y.Item2 as TamedCreature[]).Select(y =>
//                    {
//                        y.SpeciesClass = x.Key;
//                        y.SpeciesName = classes?.FirstOrDefault(z => z.Class.Equals(x.Key, StringComparison.OrdinalIgnoreCase))?.Name?.Replace("_Character_BP_C", "");

//                        return y;
//                    }).ToList()).Values.SelectMany(x => x).ToArray();

//                var wildcreatures = results?.Where(x => x.Item2 is WildCreature[])
//                    .GroupBy(x => x.Item1)
//                    .ToDictionary(x => x.Key, x => x.SelectMany(y => y.Item2 as WildCreature[]).Select(y =>
//                    {
//                        y.SpeciesClass = x.Key;
//                        y.SpeciesName = classes?.FirstOrDefault(z => z.Class.Equals(x.Key, StringComparison.OrdinalIgnoreCase))?.Name?.Replace("_Character_BP_C", "");

//                        return y;
//                    }).ToList()).Values.SelectMany(x => x).ToArray();

//                var tribes = results?.Where(x => x.Item2 is Tribe)?.Select(x =>
//                {
//                    var tribe = x.Item2 as Tribe;
//                    int tribeId = 0;
//                    if(int.TryParse(x.Item1, out tribeId)) tribe.Id = tribeId;

//                    ParseTribeLogs(tribe.TribeLog, tribe.Id ?? -1, tribe.Name);

//                    return tribe;
//                }).ToArray();

//                //save the latest tribe log date to database
//                if (_latestTribeLog != null)
//                {
//                    _savedstate.LatestTribeLogDay = _latestTribeLog.Day;
//                    _savedstate.LatestTribeLogTime = _latestTribeLog.Time;
//                    _savedstate.Save();
//                }

//                var players = results?.Where(x => x.Item2 is Player)?.Select(x =>
//                {
//                    var player = x.Item2 as Player;
//                    //SteamId
//                    if (string.IsNullOrEmpty(player.SteamId)) player.SteamId = x.Item1;
//                    return x.Item2 as Player;
//                }).ToArray();

//                var withoutSpeciesClassName = new List<Creature>();

//                //todo: not sure how playerid is set
//                var clusterlist = results?.Where(x => x.Item2 is Cluster)?.Select(x => x.Item2 as Cluster).ToArray();
//                var cluster = new Cluster { Creatures = clusterlist?.SelectMany(x => x.Creatures).ToArray() };
//                if (cluster.Creatures != null) Array.ForEach(cluster.Creatures, x =>
//                {
//                    if (string.IsNullOrEmpty(x.SpeciesClass)) withoutSpeciesClassName.Add(x);
//                    x.IsInCluster = true;
//                    x.Tamed = true;
//                    x.SpeciesName = classes?.FirstOrDefault(z => z.Class.Equals(x.SpeciesClass, StringComparison.OrdinalIgnoreCase))?.Name?.Replace("_Character_BP_C", "");
//                });

//                if (withoutSpeciesClassName.Count > 0)
//                {
//                    var str = string.Join(", ", withoutSpeciesClassName.Select(x => $"{{ id: {x.Id}{(x.OwnerName != null ? $", owner: {x.OwnerName}" : "")}{(x.Tribe != null ? $", tribe: {x.Tribe}" : "")}{(x.FullLevel != null ? $", level: {x.FullLevel}" : "")} }})"));
//                    Logging.Log($@"Some creatures in cluster have no species [ {str} ]", GetType(), LogLevel.DEBUG);
//                }

//                return new Tuple<ArkSpeciesStatsData, CreatureClass[], Creature[], Player[], Tribe[], Cluster, WildCreature[]>(ArkSpeciesStats.Instance.Data, classes, creatures, players, tribes, cluster, wildcreatures);
//            }
//            catch (OperationCanceledException) { throw; }
//            catch (Exception ex)
//            {
//                /* ignore all exceptions */
//                Logging.LogException("Failed to load json data", ex, GetType(), LogLevel.ERROR, ExceptionLevel.Unhandled);
//            }

//            return null;
//        }

//        /// <summary>
//        /// Parse kill logs for use in tame history
//        /// </summary>
//        private void ParseTribeLogs(string[] logs, int tribeId, string tribeName)
//        {
//            if (logs == null || tribeName == null) return;

//            TribeLog latestLog = null;
//            foreach (var log in logs)
//            {
//                TribeLog currentLog;
//                TameWasKilledTribeLog item;
//                currentLog = item = TameWasKilledTribeLog.FromLog(log);
//                if (item == null) currentLog = TribeLog.FromLog(log);

//                if (currentLog != null && (latestLog == null || (currentLog.Day > latestLog.Day || (currentLog.Day == latestLog.Day && currentLog.Time >= latestLog.Time)))) latestLog = currentLog;

//                if (item == null) continue;

//                item.TribeId = tribeId;
//                item.TribeName = tribeName;
//                TribeLogs?.Add(item);
//            }

//            if (latestLog != null && (_latestTribeLog == null || (latestLog.Day > _latestTribeLog.Day || (latestLog.Day == _latestTribeLog.Day && latestLog.Time >= _latestTribeLog.Time)))) _latestTribeLog = latestLog;
//        }

//        //private async void _watcher_DebugFakeChanged(object sender, ArkSaveFileChangedEventArgs e)
//        //{
//        //    Progress.Report($"Update triggered by watcher at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
//        //    Updating?.Invoke(this, new ContextUpdatingEventArgs(false));
//        //    //await UpdateAll(e.PathToLoad, e.SaveFilePath, e.ClusterPath);
//        //    if (await _contextUpdateSync.Execute(async (ct) => await UpdateAll(ct, e.PathToLoad, e.SaveFilePath, e.ClusterPath)))
//        //        Progress.Report($"Context update complete!");
//        //    else
//        //        Progress.Report($"Context update cancelled/failed.");
//        //}

//        private async Task UpdateAll(CancellationToken ct, string jsonPathOverride = null, string saveFilePathOverride = null, string clusterPathOverride = null)
//        {
//            TribeLogs = new List<TribeLog>();
//            var data = await ExtractAndLoadData(ct, false, jsonPathOverride, saveFilePathOverride, clusterPathOverride);
//            if (data != null)
//            {
//                ArkSpeciesStatsData = data.Item1 ?? new ArkSpeciesStatsData();
//                Classes = data.Item2 ?? new CreatureClass[] { };
//                Creatures = data.Item3 ?? new Creature[] { };
//                Players = data.Item4 ?? new Player[] { };
//                Tribes = data.Item5 ?? new Tribe[] { };
//                Cluster = data.Item6 ?? new Cluster();
//                Wild = data.Item7 ?? new Creature[] { };

//                IsInitialized = true;
//                LastUpdate = DateTime.Now;

//                //LogWildCreatures(ct);
//                //LogTamedCreaturesNew(ct);

//                Updated?.Invoke(this, EventArgs.Empty);
//            }
//        }

//        //private void LogWildCreatures(CancellationToken ct)
//        //{
//        //    ct.ThrowIfCancellationRequested();

//        //    try
//        //    {
//        //        //using (var scope = new TransactionScope())
//        //        //{
//        //            using (var context = _databaseContextFactory.Create())
//        //            {
//        //                //add wild creatures to database
//        //                var wild = Wild?.GroupBy(x => x.SpeciesClass).Select(x => new Database.Model.WildCreatureLogEntry { Key = x.Key, Count = x.Count(), Ids = x.Select(y => y.Id).ToArray() }).ToArray();
//        //                if (wild == null || wild.Length <= 0) return;

//        //                var removeBefore = DateTime.Now.AddDays(-7);
//        //                foreach (var item in context.WildCreatureLogs.Where(x => x.When < removeBefore).ToArray())
//        //                {
//        //                    context.WildCreatureLogs.Remove(item);
//        //                }
//        //                context.WildCreatureLogs.Add(new Database.Model.WildCreatureLog { When = LastUpdate, Entries = wild });
//        //                context.SaveChanges();
//        //            }

//        //        //    scope.Complete();
//        //        //}
//        //    }
//        //    catch (TransactionAbortedException ex)
//        //    {
//        //        Logging.LogException("Transaction aborted", ex, GetType(), LogLevel.DEBUG, ExceptionLevel.Unhandled);
//        //    }
//        //}

//        //private void LogTamedCreaturesNew(CancellationToken ct)
//        //{
//        //    ct.ThrowIfCancellationRequested();

//        //    var st = Stopwatch.StartNew();
//        //    using (var conn = new System.Data.SqlServerCe.SqlCeConnection(_constants.DatabaseConnectionString))
//        //    {
//        //        conn.Open();

//        //        using (var trans = conn.BeginTransaction())
//        //        {
//        //            try
//        //            {
//        //                var l = new Database.Model.TamedCreatureLogEntry();

//        //                //add tamed creatures to database
//        //                var tamed = CreaturesNoRaft?.OrderBy(x => x.TamedTime).GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToArray();
//        //                if (tamed == null || tamed.Length == 0) return;

//        //                using (var command = conn.CreateCommand())
//        //                {
//        //                    command.CommandText = "SELECT * FROM TamedCreatureLogEntries";
//        //                    command.CommandType = System.Data.CommandType.Text;

//        //                    using (var resultSet = command.ExecuteResultSet(System.Data.SqlServerCe.ResultSetOptions.Scrollable | System.Data.SqlServerCe.ResultSetOptions.Updatable))
//        //                    {
//        //                        var ordinal = new Func<string, int>(x => resultSet.GetOrdinal(x));

//        //                        var getValues = new Func<Creature, object[]>(tame =>
//        //                        {
//        //                            var maxFood = CalculateMaxStat(ArkSpeciesStatsData.Stat.Food, tame.SpeciesClass ?? tame.SpeciesName, tame.WildLevels?.Food, tame.TamedLevels?.Food, tame.ImprintingQuality, tame.TamedIneffectivenessModifier);
//        //                            var maxHealth = CalculateMaxStat(ArkSpeciesStatsData.Stat.Health, tame.SpeciesClass ?? tame.SpeciesName, tame.WildLevels?.Health, tame.TamedLevels?.Health, tame.ImprintingQuality, tame.TamedIneffectivenessModifier);
//        //                            var currentFood = tame.CurrentFood.HasValue ? (double)tame.CurrentFood.Value : maxFood; //todo: does no value actually mean max or is it the opposite (no food)
//        //                            var currentHealth = tame.CurrentHealth.HasValue ? (double)tame.CurrentHealth.Value : maxHealth; //todo: does no value actually mean max or is it the opposite (no health)
//        //                            var approxFoodPercentage = maxFood.HasValue && currentFood.HasValue && maxFood.Value > 0 ? (currentFood.Value / maxFood.Value).Clamp(min: 0, max: 1) : (double?)null;
//        //                            var approxHealthPercentage = maxHealth.HasValue && currentHealth.HasValue && maxFood.Value > 0 ? (currentHealth.Value / maxHealth.Value).Clamp(min: 0, max: 1) : (double?)null;
//        //                            return new[] {
//        //                                new { ordinal = ordinal(nameof(l.Id)), value = (object)tame.Id },
//        //                                new { ordinal = ordinal(nameof(l.LastSeen)), value = (object)LastUpdate },
//        //                                new { ordinal = ordinal(nameof(l.Latitude)), value = (object)tame.Latitude },
//        //                                new { ordinal = ordinal(nameof(l.Longitude)), value = (object)tame.Longitude },
//        //                                new { ordinal = ordinal(nameof(l.X)), value = (object)tame.X },
//        //                                new { ordinal = ordinal(nameof(l.Y)), value = (object)tame.Y },
//        //                                new { ordinal = ordinal(nameof(l.Z)), value = (object)tame.Z },
//        //                                new { ordinal = ordinal(nameof(l.Name)), value = (object)tame.Name },
//        //                                new { ordinal = ordinal(nameof(l.Team)), value = (object)tame.Team },
//        //                                new { ordinal = ordinal(nameof(l.PlayerId)), value = (object)tame.PlayerId },
//        //                                new { ordinal = ordinal(nameof(l.Tribe)), value = (object)tame.Tribe },
//        //                                new { ordinal = ordinal(nameof(l.OwnerName)), value = (object)tame.OwnerName },
//        //                                new { ordinal = ordinal(nameof(l.Tamer)), value = (object)tame.Tamer },
//        //                                new { ordinal = ordinal(nameof(l.Female)), value = (object)tame.Female },
//        //                                new { ordinal = ordinal(nameof(l.BaseLevel)), value = (object)tame.BaseLevel },
//        //                                new { ordinal = ordinal(nameof(l.FullLevel)), value = (object)tame.FullLevel },
//        //                                new { ordinal = ordinal(nameof(l.Experience)), value = (object)tame.Experience },
//        //                                new { ordinal = ordinal(nameof(l.ApproxFoodPercentage)), value = (object)approxFoodPercentage },
//        //                                new { ordinal = ordinal(nameof(l.ApproxHealthPercentage)), value = (object)approxHealthPercentage },
//        //                                new { ordinal = ordinal(nameof(l.ImprintingQuality)), value = (object)tame.ImprintingQuality },
//        //                                new { ordinal = ordinal(nameof(l.TamedAtTime)), value = (object)tame.TamedAtTime },
//        //                                new { ordinal = ordinal(nameof(l.TamedTime)), value = (object)tame.TamedTime },
//        //                                new { ordinal = ordinal(nameof(l.RelatedLogEntries)), value = (object)null },
//        //                                new { ordinal = ordinal(nameof(l.SpeciesClass)), value = (object)tame.SpeciesClass },
//        //                                new { ordinal = ordinal(nameof(l.IsConfirmedDead)), value = (object)false },
//        //                                new { ordinal = ordinal(nameof(l.IsInCluster)), value = (object)false },
//        //                                new { ordinal = ordinal(nameof(l.IsUnavailable)), value = (object)false }
//        //                                }.OrderBy(x => x.ordinal).Select(x => x.value).ToArray();
//        //                        });

//        //                        var ids = tamed.Select(x => x.Id).ToList();
//        //                        if (resultSet.HasRows)
//        //                        {
//        //                            resultSet.ReadFirst();

//        //                            do
//        //                            {
//        //                                ct.ThrowIfCancellationRequested();

//        //                                var id = resultSet.SafeGet<long>(nameof(l.Id));
//        //                                var tame = tamed.FirstOrDefault(x => x.Id == id);
//        //                                if (tame == null)
//        //                                {
//        //                                    var isUnavailable = resultSet.SafeGet<bool>(nameof(l.IsUnavailable));
//        //                                    var isInCluster = resultSet.SafeGet<bool>(nameof(l.IsInCluster));
//        //                                    var isNowInCluster = Cluster?.Creatures.Any(x => x.Id == id) ?? false;
//        //                                    if (isUnavailable == false || (isInCluster == false && isNowInCluster == true))
//        //                                    {
//        //                                        //this tame is now dead, missing or uploaded we should update the state
//        //                                        var name = resultSet.SafeGet<string>(nameof(l.Name));
//        //                                        var team = resultSet.SafeGet<int?>(nameof(l.Team));
//        //                                        var speciesClass = resultSet.SafeGet<string>(nameof(l.SpeciesClass));
//        //                                        var fullLevel = resultSet.SafeGet<int?>(nameof(l.FullLevel));
//        //                                        var baseLevel = resultSet.SafeGet<int>(nameof(l.BaseLevel));
//        //                                        var isConfirmedDead = resultSet.SafeGet<bool>(nameof(l.IsConfirmedDead));

//        //                                        //get any kill logs that could relate to this creature.
//        //                                        //since tribe logs do not include the id of the creature, we have to make an informed guess
//        //                                        var relatedLogs = team.HasValue && speciesClass != null ? TribeLogs?.OfType<TameWasKilledTribeLog>()
//        //                                            .Where(x =>
//        //                                                    (x.Day > _savedstate.LatestTribeLogDay || (x.Day == _savedstate.LatestTribeLogDay && x.Time >= _savedstate.LatestTribeLogTime))
//        //                                                    && x.TribeId == team.Value
//        //                                                    && (!string.IsNullOrWhiteSpace(name) ? name.Equals(x.Name, StringComparison.Ordinal) : x.Name.Equals(x.SpeciesName, StringComparison.Ordinal))
//        //                                                    && x.Level >= (fullLevel ?? baseLevel)
//        //                                                    && ((ArkSpeciesAliases.Instance.GetAliases(x.SpeciesName)?.Contains(speciesClass)) ?? false)).ToArray() : null;

//        //                                        //is in cluster
//        //                                        resultSet.SetBoolean(ordinal(nameof(l.IsInCluster)), isNowInCluster);

//        //                                        //is confirmed dead
//        //                                        isConfirmedDead = (isNowInCluster == false && (relatedLogs != null && relatedLogs.Length > 0));
//        //                                        resultSet.SetBoolean(ordinal(nameof(l.IsConfirmedDead)), isConfirmedDead);

//        //                                        //is unavailable
//        //                                        resultSet.SetBoolean(ordinal(nameof(l.IsUnavailable)), true);

//        //                                        //related logs
//        //                                        if (isConfirmedDead)
//        //                                        {
//        //                                            //this could end up picking the wrong log, or the same log for two creatures, but we lack the information to make a better guess
//        //                                            var mostProbableRelatedLog = relatedLogs.OrderBy(x => x.Level - (fullLevel ?? baseLevel)).FirstOrDefault()?.Raw;
//        //                                            resultSet.SetString(ordinal(nameof(l.RelatedLogEntries)), mostProbableRelatedLog);
//        //                                        }

//        //                                        resultSet.Update();
//        //                                    }

//        //                                    continue;
//        //                                }

//        //                                ids.Remove(id);
//        //                                resultSet.SetValues(getValues(tame).Select(x => x == null ? DBNull.Value : x).ToArray());
//        //                                resultSet.Update();
//        //                            } while (resultSet.Read());
//        //                        }

//        //                        foreach (var id in ids)
//        //                        {
//        //                            ct.ThrowIfCancellationRequested();

//        //                            var tame = tamed.FirstOrDefault(x => x.Id == id);
//        //                            if (tame == null) continue;

//        //                            var record = resultSet.CreateRecord();
//        //                            record.SetValues(getValues(tame).Select(x => x == null ? DBNull.Value : x).ToArray());

//        //                            resultSet.Insert(record, System.Data.SqlServerCe.DbInsertOptions.PositionOnInsertedRow);
//        //                        }
//        //                    }
//        //                }

//        //                trans.Commit();
//        //            }
//        //            catch
//        //            {
//        //                trans.Rollback();
//        //            }
//        //            finally
//        //            {
//        //                conn.Close();
//        //            }
//        //        }
//        //    }
//        //    Debug.WriteLine($"{nameof(LogTamedCreaturesNew)} finished in {st.ElapsedMilliseconds:N0} ms");
//        //}

//        //sqlcompact have crap performance, no support for clustered indexes, attaching entities takes a milion years
//        //this code is the fastest way to achieve this using ef (note the query to get all entities from database rather than specific ids)
//        //private void LogTamedCreatures()
//        //{
//        //    var st = Stopwatch.StartNew();
//        //    using (var context = _databaseContextFactory.Create())
//        //    {
//        //        //add tamed creatures to database
//        //        var tamed = Creatures?.OrderBy(x => x.TamedTime).GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToArray();
//        //        var ids = tamed?.Select(x => x.Id).ToArray();
//        //        if (ids == null || ids.Length == 0) return;

//        //        //var duplicateIds = ids.GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.Key).ToArray();
//        //        //var duplicateTames = duplicateIds.Select(x => tamed.Where(y => y.Id == x).ToArray()).ToArray();

//        //        var st2 = Stopwatch.StartNew();
//        //        var fromDatabase = context.TamedCreatureLogEntries.ToArray(); //.Where(x => ids.Contains(x.Id)).ToArray();
//        //        //var idsFromDatabase = context.TamedCreatureLogEntries.Select(x => x.Id).ToArray();
//        //        Debug.WriteLine($"{nameof(LogTamedCreatures)} get all from database finished in {st2.ElapsedMilliseconds:N0} ms");

//        //        var st3 = Stopwatch.StartNew();
//        //        //context.TamedCreatureLogEntries.AddOrUpdate(
//        //        //    x => x.Id,
//        //        //    tamed.Select(tame => new Database.Model.TamedCreatureLogEntry
//        //        //    {
//        //        //        Id = tame.Id,
//        //        //        LastSeen = LastUpdate,
//        //        //        Latitude = tame.Latitude,
//        //        //        Longitude = tame.Longitude,
//        //        //        X = tame.X,
//        //        //        Y = tame.Y,
//        //        //        Z = tame.Z,
//        //        //        Name = tame.Name,
//        //        //        Team = tame.Team,
//        //        //        PlayerId = tame.PlayerId,
//        //        //        Tribe = tame.Tribe,
//        //        //        OwnerName = tame.OwnerName,
//        //        //        Tamer = tame.Tamer,
//        //        //        Female = tame.Female,
//        //        //        BaseLevel = tame.BaseLevel,
//        //        //        FullLevel = tame.FullLevel,
//        //        //        Experience = tame.Experience,
//        //        //        CurrentFood = tame.CurrentFood,
//        //        //        ImprintingQuality = tame.ImprintingQuality,
//        //        //        TamedAtTime = tame.TamedAtTime,
//        //        //        TamedTime = tame.TamedTime,
//        //        //        RelatedLogEntry = null //todo: fix
//        //        //    }).ToArray());

//        //        foreach (var id in ids)
//        //        {
//        //            var tame = tamed.FirstOrDefault(x => x.Id == id);
//        //            if (tame == null) continue;

//        //            //var dbo = new Database.Model.TamedCreatureLogEntry
//        //            //{
//        //            //    Id = tame.Id,
//        //            //    LastSeen = LastUpdate,
//        //            //    Latitude = tame.Latitude,
//        //            //    Longitude = tame.Longitude,
//        //            //    X = tame.X,
//        //            //    Y = tame.Y,
//        //            //    Z = tame.Z,
//        //            //    Name = tame.Name,
//        //            //    Team = tame.Team,
//        //            //    PlayerId = tame.PlayerId,
//        //            //    Tribe = tame.Tribe,
//        //            //    OwnerName = tame.OwnerName,
//        //            //    Tamer = tame.Tamer,
//        //            //    Female = tame.Female,
//        //            //    BaseLevel = tame.BaseLevel,
//        //            //    FullLevel = tame.FullLevel,
//        //            //    Experience = tame.Experience,
//        //            //    CurrentFood = tame.CurrentFood,
//        //            //    ImprintingQuality = tame.ImprintingQuality,
//        //            //    TamedAtTime = tame.TamedAtTime,
//        //            //    TamedTime = tame.TamedTime,
//        //            //    RelatedLogEntry = null //todo: fix
//        //            //};

//        //            //if (idsFromDatabase.Any(x => x == id))
//        //            //{
//        //            //    context.TamedCreatureLogEntries.Attach(dbo);
//        //            //    context.Entry(dbo).State = System.Data.Entity.EntityState.Modified;
//        //            //}
//        //            //else
//        //            //{
//        //            //    context.TamedCreatureLogEntries.Add(dbo);
//        //            //}

//        //            //var dbo = context.TamedCreatureLogEntries.SingleOrDefault(x => x.Id == id);
//        //            var dbo = fromDatabase.FirstOrDefault(x => x.Id == id);
//        //            if (dbo == null)
//        //            {
//        //                //does seem to be seconds from when the server was created rather than a full date
//        //                //var tamedAt = (DateTime?)null;
//        //                //if (tame.TamedAtTime.HasValue)
//        //                //{
//        //                //    tamedAt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds((double)tame.TamedAtTime.Value);
//        //                //}

//        //                //not yet logged in database
//        //                context.TamedCreatureLogEntries.Add(new Database.Model.TamedCreatureLogEntry
//        //                {
//        //                    Id = tame.Id,
//        //                    LastSeen = LastUpdate,
//        //                    Latitude = tame.Latitude,
//        //                    Longitude = tame.Longitude,
//        //                    X = tame.X,
//        //                    Y = tame.Y,
//        //                    Z = tame.Z,
//        //                    Name = tame.Name,
//        //                    Team = tame.Team,
//        //                    PlayerId = tame.PlayerId,
//        //                    Tribe = tame.Tribe,
//        //                    OwnerName = tame.OwnerName,
//        //                    Tamer = tame.Tamer,
//        //                    Female = tame.Female,
//        //                    BaseLevel = tame.BaseLevel,
//        //                    FullLevel = tame.FullLevel,
//        //                    Experience = tame.Experience,
//        //                    CurrentFood = tame.CurrentFood,
//        //                    ImprintingQuality = tame.ImprintingQuality,
//        //                    TamedAtTime = tame.TamedAtTime,
//        //                    TamedTime = tame.TamedTime,
//        //                    RelatedLogEntry = null //todo: fix
//        //                });
//        //            }
//        //            else
//        //            {
//        //                //if required, update with new information
//        //                dbo.LastSeen = LastUpdate;
//        //                dbo.Latitude = tame.Latitude;
//        //                dbo.Longitude = tame.Longitude;
//        //                dbo.X = tame.X;
//        //                dbo.Y = tame.Y;
//        //                dbo.Z = tame.Z;
//        //                dbo.Name = tame.Name;
//        //                dbo.Team = tame.Team;
//        //                dbo.PlayerId = tame.PlayerId;
//        //                dbo.Tribe = tame.Tribe;
//        //                dbo.OwnerName = tame.OwnerName;
//        //                dbo.Tamer = tame.Tamer;
//        //                dbo.Female = tame.Female;
//        //                dbo.BaseLevel = tame.BaseLevel;
//        //                dbo.FullLevel = tame.FullLevel;
//        //                dbo.Experience = tame.Experience;
//        //                dbo.CurrentFood = tame.CurrentFood;
//        //                dbo.ImprintingQuality = tame.ImprintingQuality;
//        //                dbo.TamedAtTime = tame.TamedAtTime;
//        //                dbo.TamedTime = tame.TamedTime;
//        //                dbo.RelatedLogEntry = null; //todo: fix
//        //            }
//        //        }
//        //        Debug.WriteLine($"{nameof(LogTamedCreatures)} add/update finished in {st3.ElapsedMilliseconds:N0} ms");
//        //        var st4 = Stopwatch.StartNew();
//        //        context.SaveChanges();
//        //        Debug.WriteLine($"{nameof(LogTamedCreatures)} save finished in {st4.ElapsedMilliseconds:N0} ms");
//        //    }
//        //    st.Stop();
//        //    Debug.WriteLine($"{nameof(LogTamedCreatures)} finished in {st.ElapsedMilliseconds:N0} ms");
//        //}

//        public static double? CalculateMaxStat(ArkSpeciesStatsData.Stat stat, string speciesNameOrClass, int? wildLevelStat, int? tamedLevelStat, decimal? imprintingQuality, decimal? tamedIneffectivenessModifier)
//        {
//            var speciesAliases = ArkSpeciesAliases.Instance.GetAliases(speciesNameOrClass) ?? new[] { speciesNameOrClass };
//            return ArkSpeciesStats.Instance.Data?.GetMaxValue(
//                            speciesAliases, //a list of alternative species names
//                            stat,
//                            wildLevelStat ?? 0,
//                            tamedLevelStat ?? 0,
//                            (double)(1 / (1 + (tamedIneffectivenessModifier ?? 0m))),
//                            (double)(imprintingQuality ?? 0m));
//        }

//        public string GetElevationAsText(decimal z)
//        {
//            //for the island
//            //550	Map ceiling (insta-death)
//            //460	Volcano's Peak
//            //0	Nothing special
//            //-145	Sea level
//            //-285	Lowest(?) point on land
//            //-460	Sea floor
//            //-480	Map floor (insta-death)

//            var elevation = (double)z / 100d;
//            var amsl = elevation + 145;
//            var hint = elevation <= -480 ? "below world" : elevation <= -450 ? "sea floor" : elevation <= -285 ? "underground" : elevation >= -165 && elevation <= -125 ? "sea level" : elevation >= 550 ? "above world" : elevation >= 525 ? "map ceiling" : null;

//            return $"{amsl:N0} meters" + (hint != null ? $" (~{hint})" : "");
//        }

//        public void DisableContextUpdates()
//        {
//            _contextUpdatesDisabledOverride = true;
//        }
//        public void EnableContextUpdates()
//        {
//            _contextUpdatesDisabledOverride = false;
//        }

//        #region IDisposable Support
//        private bool disposedValue = false; // To detect redundant calls

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {
//                    // TODO: dispose managed state (managed objects).
//                    _watcher?.Dispose();
//                    _watcher = null;
//                }

//                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
//                // TODO: set large fields to null.

//                disposedValue = true;
//            }
//        }

//        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
//        // ~ArkContext() {
//        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
//        //   Dispose(false);
//        // }

//        // This code added to correctly implement the disposable pattern.
//        public void Dispose()
//        {
//            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
//            Dispose(true);
//            // TODO: uncomment the following line if the finalizer is overridden above.
//            // GC.SuppressFinalize(this);
//        }
//        #endregion
//    }
//}
