using ArkBot.Data;
using ArkBot.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArkBot
{
    public class ArkContext : IDisposable
    {
        private ArkSaveFileWatcher _watcher;
        private Config _config;
        private IProgress<string> _progress;

        private string _jsonOutputDirPathTamed => _config?.JsonOutputDirPath != null ? Path.Combine(_config.JsonOutputDirPath, "tamed") : null;
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
        public Tribe[] Tribes { get; private set; }
        public Player[] Players { get; private set; }
        public CreatureClass[] Classes { get; private set; }
        public Cluster Cluster { get; private set; }

        public ArkContext(Config config, IProgress<string> progress)
        {
            _config = config;
            _progress = progress;
        }

        public async Task Initialize(ArkSpeciesAliases aliases = null)
        {
            _progress.Report("Context initialization started...");

            SpeciesAliases = aliases ?? await ArkSpeciesAliases.Load();

            var data = await ExtractAndLoadData();
            if (data != null)
            {
                ArkSpeciesStatsData = data.Item1;
                Classes = data.Item2;
                Creatures = data.Item3;
                Players = data.Item4;
                Tribes = data.Item5;
                Cluster = data.Item6;
                _lastUpdate = DateTime.Now; //set backing field in order to not trigger property logic, which would have added the "initialization" time to the last updated collection
            }

            if (!_config.DebugNoExtract)
            {
                _watcher = new ArkSaveFileWatcher { SaveFilePath = _config.SaveFilePath };
                _watcher.Changed += _watcher_Changed;
            }
        }

        private async Task<Tuple<ArkSpeciesStatsData, CreatureClass[], Creature[], Player[], Tribe[], Cluster>> ExtractAndLoadData()
        {
            //extract the save file data to json using ark-tools
            _progress.Report("Extracting ARK gamedata...");
            if (!_config.DebugNoExtract && !await ExtractSaveFileData()) return null;

            //load the resulting json into memory
            _progress.Report("Loading json data into memory...");
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
                new { inpath = _config.SaveFilePath, path = _jsonOutputDirPathTribes, filepathCheck = _rJson, verb = "tribes", parameters = "--structures --items --tribeless" },
                new { inpath = _config.SaveFilePath, path = _jsonOutputDirPathPlayers, filepathCheck = _rJson, verb = "players", parameters = "--inventory --positions --no-privacy" }, // --max-age 2592000 //limit to last 30 days
                new { inpath = _config.ClusterSavePath, path = _jsonOutputDirPathCluster, filepathCheck = _rCluster, verb = "cluster", parameters = "" }
            })
            {
                _progress.Report($"- {action.verb}");

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

        private async Task<Tuple<ArkSpeciesStatsData, CreatureClass[], Creature[], Player[], Tribe[], Cluster>> LoadDataFromJson()
        {
            try
            {
                var results = new List<Tuple<string, object>>();

                var actions = new[]
                {
                    new
                    {
                        path = _jsonOutputDirPathTamed,
                        selector = (Func<string, Type>)((x) => x.Equals("classes", StringComparison.OrdinalIgnoreCase) ? typeof(CreatureClass[]) : typeof(Creature[]))
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
                var creatures = results?.Where(x => x.Item2 is Creature[])
                    .GroupBy(x => x.Item1)
                    .ToDictionary(x => x.Key, x => x.SelectMany(y => y.Item2 as Creature[]).Select(y =>
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
                    return tribe;
                }).ToArray();
                var players = results?.Where(x => x.Item2 is Player)?.Select(x => x.Item2 as Player).ToArray();

                //todo: not sure how playerid is set
                var clusterlist = results?.Where(x => x.Item2 is Cluster)?.Select(x => x.Item2 as Cluster).ToArray();
                var cluster = new Cluster { Creatures = clusterlist?.SelectMany(x => x.Creatures).ToArray() };

                return new Tuple<ArkSpeciesStatsData, CreatureClass[], Creature[], Player[], Tribe[], Cluster>(arkSpeciesStatsData, classes, creatures, players, tribes, cluster);
            }
            catch { /* ignore all exceptions */ }

            return null;
        }

        private async void _watcher_Changed(object sender, ArkSaveFileChangedEventArgs e)
        {
            _progress.Report($"Update triggered by watcher at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            var data = await ExtractAndLoadData();
            if (data != null)
            {
                ArkSpeciesStatsData = data.Item1;
                Classes = data.Item2;
                Creatures = data.Item3;
                Players = data.Item4;
                Tribes = data.Item5;
                Cluster = data.Item6;

                LastUpdate = DateTime.Now;
            }
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
