using ArkBot.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class ArkContext : IDisposable
    {
        private ArkSaveFileWatcher _watcher;
        private string _saveFilePath;
        private string _arktoolsExecutablePath;
        private string _jsonOutputDirPath;
        private bool _debugNoExtract;

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

        public Creature[] Creatures { get; private set; }

        public ArkContext(string saveFilePath, string arktoolsExecutablePath, string jsonOutputDirPath, bool debugNoExtract = false)
        {
            _saveFilePath = saveFilePath;
            _arktoolsExecutablePath = arktoolsExecutablePath;
            _jsonOutputDirPath = jsonOutputDirPath;
            _debugNoExtract = debugNoExtract;
        }

        public async Task Load()
        {
            var creatures = await ExtractAndLoadData();
            if (creatures != null)
            {
                Creatures = creatures;
                LastUpdate = DateTime.Now;
            }

            if (!_debugNoExtract)
            {
                _watcher = new ArkSaveFileWatcher { SaveFilePath = _saveFilePath };
                _watcher.Changed += _watcher_Changed;
            }
        }

        private async Task<Creature[]> ExtractAndLoadData()
        {
            //exctract the save file data to json using ark-tools
            if (!_debugNoExtract && !await ExtractSaveFileData()) return null;

            //load the resulting json into memory
            var creatures = await LoadDataFromJson();

            return creatures;
        }

        private async Task<bool> ExtractSaveFileData()
        {
            try
            {
                foreach (var filepath in Directory.GetFiles(_jsonOutputDirPath, "*.json", SearchOption.TopDirectoryOnly))
                {
                    File.Delete(filepath);
                }

                var tcs = new TaskCompletionSource<int>();
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Verb = "runas",
                    Arguments = $@"/C {_arktoolsExecutablePath} tamed ""{_saveFilePath}"" ""{_jsonOutputDirPath}"" --pretty-printing",
                    WorkingDirectory = Directory.GetParent(_arktoolsExecutablePath).FullName,
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
                if (exitCode == 0) return true;
            }
            catch { /* ignore all exceptions */ }

            return false;
        }

        private async Task<Creature[]> LoadDataFromJson()
        {
            try
            {
                var classes = (CreatureClass[])null;
                var creatures = new Dictionary<string, List<Creature>>();

                foreach (var file in Directory.EnumerateFiles(_jsonOutputDirPath, "*.json", SearchOption.TopDirectoryOnly))
                {
                    if (Path.GetFileName(file).Equals("classes.json", StringComparison.OrdinalIgnoreCase))
                    {
                        if (classes != null) continue;
                        classes = JsonConvert.DeserializeObject<CreatureClass[]>(File.ReadAllText(file));
                    }
                    else
                    {
                        var className = Path.GetFileNameWithoutExtension(file);

                        using (var reader = File.OpenText(file))
                        {
                            var tmp = JsonConvert.DeserializeObject<Creature[]>(await reader.ReadToEndAsync());

                            if (!creatures.ContainsKey(className)) creatures.Add(className, new List<Creature>());
                            creatures[className].AddRange(tmp);
                        }
                    }
                }

                //map the creature data
                return creatures.SelectMany(x =>
                {
                    x.Value.ForEach(y =>
                    {
                        y.SpeciesClass = x.Key;
                        y.SpeciesName = classes.FirstOrDefault(z => z.Class.Equals(x.Key, StringComparison.OrdinalIgnoreCase))?.Name?.Replace("_Character_BP_C", "");
                    });

                    return x.Value;
                }).ToArray();
            }
            catch { /* ignore all exceptions */ }

            return null;
        }

        private async void _watcher_Changed(object sender, ArkSaveFileChangedEventArgs e)
        {
            var creatures = await ExtractAndLoadData();
            if (creatures != null)
            {
                Creatures = creatures;
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
