using ArkBot.Database.Model;
using ArkBot.Services;
using ArkBot.Services.Data;
using ArkBot.Steam;
using ArkBot.Threading;
using ArkSavegameToolkitNet;
using ArkSavegameToolkitNet.Domain;
using ArkSavegameToolkitNet.Structs;
using ArkSavegameToolkitNet.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Ark
{
    public class ArkServerContext : IDisposable
    {
        public ServerConfigSection Config { get; set; }

        internal IArkSaveFileWatcher _saveFileWatcher;
        internal ArkContextManager _contextManager;

        //public event UpdateTriggeredEventHandler UpdateQueued;
        public event UpdateCompletedEventHandler UpdateCompleted;
        public event BackupCompletedEventHandler BackupCompleted;
        public event VoteInitiatedEventHandler VoteInitiated;
        public event VoteResultForcedEventHandler VoteResultForced;

        public bool IsInitialized { get; set; }

        public IEnumerable<ArkTamedCreature> NoRafts => TamedCreatures?.Where(x => !x.ClassName.Equals("Raft_BP_C"));

        public ArkTamedCreature[] TamedCreatures { get; set; }
        public ArkWildCreature[] WildCreatures { get; set; }
        public ArkSavegameToolkitNet.Domain.ArkTribe[] Tribes { get; set; }
        public ArkPlayer[] Players { get; set; }
        public ArkItem[] Items { get; set; }
        public ArkStructure[] Structures { get; set; }

        public SteamManager Steam { get; private set; }

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

        public ArkServerContext(ServerConfigSection config)
        {
            Config = config;
            _saveFileWatcher = new ArkSaveFileWatcher(this);
            Steam = new SteamManager(config);
        }

        internal bool _serverUpdate(bool manualUpdate, IConfig fullconfig, ISavegameBackupService savegameBackupService, IProgress<string> progress, CancellationToken ct)
        {
            //backup this savegame
            if (!manualUpdate)
            {
                SavegameBackupResult result = null;
                try
                {
                    if (fullconfig.BackupsEnabled)
                    {
                        result = savegameBackupService.CreateBackup(Config, _contextManager?.GetCluster(Config.Cluster)?.Config);
                        if (result != null && result.ArchivePaths != null) progress.Report($@"Server ({Config.Key}): Backup successfull ({(string.Join(", ", result.ArchivePaths.Select(x => $@"""{x}""")))})!");
                        else progress.Report($"Server ({Config.Key}): Backup failed...");
                    }
                }
                catch (Exception ex) { Logging.LogException($"Server ({Config.Key}): Backup failed", ex, typeof(ArkServerContext), LogLevel.ERROR, ExceptionLevel.Ignored); }
                BackupCompleted?.Invoke(this, fullconfig.BackupsEnabled, result);
            }


            //todo: temp copy all
            var copy = true;
            var success = false;
            var cancelled = false;
            var tmppaths = new List<string>();
            var gid = Guid.NewGuid().ToString();
            var tempFileOutputDirPath = Path.Combine(fullconfig.TempFileOutputDirPath, gid);
            ArkSavegame save = null;
            var st = Stopwatch.StartNew();
            try
            {
                progress.Report($"Server ({Config.Key}): Update started ({DateTime.Now:HH:mm:ss.ffff})");

                var directoryPath = Path.GetDirectoryName(Config.SaveFilePath);
                if (copy)
                {
                    //todo: if it exists get a new path
                    if (!Directory.Exists(tempFileOutputDirPath)) Directory.CreateDirectory(tempFileOutputDirPath);
                }

                if (copy)
                {
                    var saveFilePath = Path.Combine(tempFileOutputDirPath, Path.GetFileName(Config.SaveFilePath));
                    tmppaths.Add(saveFilePath);
                    File.Copy(Config.SaveFilePath, saveFilePath);

                    save = new ArkSavegame(saveFilePath);
                }
                else save = new ArkSavegame(Config.SaveFilePath);
                save.LoadEverything();
                ct.ThrowIfCancellationRequested();

                ArkSavegameToolkitNet.ArkTribe[] tribes = null;
                if (copy)
                {
                    var tribePaths = new List<string>();
                    foreach (var tp in Directory.GetFiles(directoryPath, "*.arktribe", SearchOption.TopDirectoryOnly))
                    {
                        var tribePath = Path.Combine(tempFileOutputDirPath, Path.GetFileName(tp));
                        tribePaths.Add(tp);
                        tmppaths.Add(tribePath);
                        File.Copy(tp, tribePath);
                    }
                    tribes = tribePaths.Select(x => new ArkSavegameToolkitNet.ArkTribe(x)).ToArray();
                }
                else tribes = Directory.GetFiles(directoryPath, "*.arktribe", SearchOption.TopDirectoryOnly).Select(x => new ArkSavegameToolkitNet.ArkTribe(x)).ToArray();
                ct.ThrowIfCancellationRequested();

                ArkProfile[] profiles = null;
                if (copy)
                {
                    var profilePaths = new List<string>();
                    foreach (var pp in Directory.GetFiles(directoryPath, "*.arkprofile", SearchOption.TopDirectoryOnly))
                    {
                        var profilePath = Path.Combine(tempFileOutputDirPath, Path.GetFileName(pp));
                        profilePaths.Add(pp);
                        tmppaths.Add(profilePath);
                        File.Copy(pp, profilePath);
                    }
                    profiles = profilePaths.Select(x => new ArkProfile(x)).ToArray();
                }
                else profiles = Directory.GetFiles(directoryPath, "*.arkprofile", SearchOption.TopDirectoryOnly).Select(x => new ArkProfile(x)).ToArray();
                ct.ThrowIfCancellationRequested();

                var _myCharacterStatusComponent = ArkName.Create("MyCharacterStatusComponent");
                var statusComponents = save.Objects.Where(x => x.IsDinoStatusComponent).ToDictionary(x => x.Index, x => x);
                var tamed = save.Objects.Where(x => x.IsTamedCreature).Select(x =>
                {
                    GameObject status = null;
                    statusComponents.TryGetValue(x.GetPropertyValue<ObjectReference>(_myCharacterStatusComponent).ObjectId, out status);
                    return x.AsTamedCreature(status, null, save.SaveState);
                }).ToArray();
                var wild = save.Objects.Where(x => x.IsWildCreature).Select(x =>
                {
                    GameObject status = null;
                    statusComponents.TryGetValue(x.GetPropertyValue<ObjectReference>(_myCharacterStatusComponent).ObjectId, out status);
                    return x.AsWildCreature(status, null, save.SaveState);
                }).ToArray();

                var _myData = ArkName.Create("MyData");
                var _playerDataID = ArkName.Create("PlayerDataID");
                var _linkedPlayerDataID = ArkName.Create("LinkedPlayerDataID");
                var playerdict = save.Objects.Where(x => x.IsPlayerCharacter).ToLookup(x => x.GetPropertyValue<ulong>(_linkedPlayerDataID), x => x);
                var duplicates = playerdict.Where(x => x.Count() > 1).ToArray();
                var players = profiles.Select(x =>
                {
                    var mydata = x.GetPropertyValue<StructPropertyList>(_myData);
                    var playerId = mydata.GetPropertyValue<ulong>(_playerDataID);
                    var player = playerdict[playerId]?.FirstOrDefault();
                    return x.Profile.AsPlayer(player, x.SaveTime, save.SaveState);
                }).ToArray();

                TamedCreatures = tamed;
                WildCreatures = wild;
                Players = players;
                Tribes = tribes.Select(x => x.Tribe.AsTribe()).ToArray();
                Items = save.Objects.Where(x => x.IsItem).Select(x => x.AsItem(save.SaveState)).ToArray();
                Structures = save.Objects.Where(x => x.IsStructure).Select(x => x.AsStructure(save.SaveState)).ToArray();

                progress.Report($"Server ({Config.Key}): Update finished in {st.ElapsedMilliseconds:N0} ms");
                IsInitialized = true;
                
                LastUpdate = DateTime.Now;
                success = true;
            }
            catch (OperationCanceledException)
            {
                progress.Report($"Server ({Config.Key}): Update was cancelled after {st.ElapsedMilliseconds:N0} ms");
                cancelled = true;
            }
            catch (Exception ex)
            {
                Logging.LogException($"Failed to update server ({Config.Key})", ex, typeof(ArkServerContext), LogLevel.ERROR, ExceptionLevel.Ignored);
                progress.Report($"Server ({Config.Key}): Update failed after {st.ElapsedMilliseconds:N0} ms");
            }
            finally
            {
                save?.Dispose();
                if (copy)
                {
                    try
                    {
                        foreach (var path in tmppaths) File.Delete(path);
                        Directory.Delete(tempFileOutputDirPath);
                    }
                    catch { /* ignore exception */ }
                }

                UpdateCompleted?.Invoke(this, success, cancelled);
            }

            GC.Collect();

            return success;
        }

        public void OnVoteInitiated(Database.Model.Vote item)
        {
            VoteInitiated?.Invoke(this, new VoteInitiatedEventArgs { Item = item });
        }
        public void OnVoteResultForced(Database.Model.Vote item, VoteResult forcedResult)
        {
            VoteResultForced?.Invoke(this, new VoteResultForcedEventArgs { Item = item, Result = forcedResult });
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;

            if (disposing)
            {
                _saveFileWatcher?.Dispose();
                _saveFileWatcher = null;

                Steam?.Dispose();
                Steam = null;
            }

            disposedValue = true;
        }
        public void Dispose() { Dispose(true); }
        private bool disposedValue = false;
        #endregion
    }
}
