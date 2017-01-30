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

        public Creature[] Creatures { get; private set; }

        public ArkContext(string saveFilePath, string arktoolsExecutablePath, string jsonOutputDirPath)
        {
            _saveFilePath = saveFilePath;
            _arktoolsExecutablePath = arktoolsExecutablePath;
            _jsonOutputDirPath = jsonOutputDirPath;
        }

        public async Task Load()
        {
            var creatures = await ExtractAndLoadData();
            if (creatures != null) Creatures = creatures;

            _watcher = new ArkSaveFileWatcher { SaveFilePath = _saveFilePath };
            _watcher.Changed += _watcher_Changed;
        }

        private async Task<Creature[]> ExtractAndLoadData()
        {
            //exctract the save file data to json using ark-tools
            if (!await ExtractSaveFileData()) return null;

            //load the reusulting json into memory
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
            var classes = (CreatureClass[])null;
            var creatures = new Dictionary<string, List<Creature>>();

            foreach (var file in Directory.EnumerateFiles(/*dir*/ _jsonOutputDirPath))
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
                    y.SpeciesName = classes.FirstOrDefault(z => z.Class.Equals(x.Key, StringComparison.OrdinalIgnoreCase))?.Name;
                });

                return x.Value;
            }).ToArray();
        }

        private async void _watcher_Changed(object sender, ArkSaveFileChangedEventArgs e)
        {
            var creatures = await ExtractAndLoadData();
            if (creatures != null) Creatures = creatures;
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
