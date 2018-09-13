using ArkBot.Configuration.Model;
using ArkBot.Database.Model;
using ArkBot.Services;
using ArkBot.Services.Data;
using ArkBot.Steam;
using ArkBot.Threading;
using ArkSavegameToolkitNet;
using ArkSavegameToolkitNet.Domain;
using ArkSavegameToolkitNet.Structs;
using ArkSavegameToolkitNet.Types;
using Autofac;
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
    public class ArkServerContext : ArkGameData, IArkUpdateableContext,  IDisposable
    {
        public ServerConfigSection Config { get; set; }

        internal IArkSaveFileWatcher _saveFileWatcher;
        internal ArkContextManager _contextManager;
        internal ArkClusterContext _clusterContext;
        private ILifetimeScope _scope;
        private ISavedState _savedState;
        private ArkAnonymizeData _anonymizeData;

        //public event UpdateTriggeredEventHandler UpdateQueued;
        public event GameDataUpdatedEventHandler GameDataUpdated;
        public event UpdateCompletedEventHandler UpdateCompleted;
        public event BackupCompletedEventHandler BackupCompleted;
        public event VoteInitiatedEventHandler VoteInitiated;
        public event VoteResultForcedEventHandler VoteResultForced;

        public bool IsInitialized { get; set; }

        //public IEnumerable<ArkTamedCreature> NoRafts => TamedCreatures?.Where(x => !x.ClassName.Equals("Raft_BP_C"));

        //public IEnumerable<ArkTamedCreature> CloudCreatures
        //{
        //    get
        //    {
        //        var cluster = _contextManager.GetCluster(Config.Cluster);
        //        var cloudCreatures = cluster?.CloudInventories?.SelectMany(x => x.Dinos);

        //        return cloudCreatures;
        //    }
        //}

        //public IEnumerable<ArkTamedCreature> InclCloud
        //{
        //    get
        //    {
        //        var cluster = _contextManager.GetCluster(Config.Cluster);
        //        var cloudCreatures = cluster?.CloudInventories?.SelectMany(x => x.Dinos);

        //        if (cloudCreatures == null) return TamedCreatures;
        //        if (TamedCreatures == null) return cloudCreatures;
        //        return cloudCreatures.Concat(TamedCreatures);
        //    }
        //}

        //public IEnumerable<ArkTamedCreature> InclCloudNoRafts
        //{
        //    get
        //    {
        //        var cluster = _contextManager.GetCluster(Config.Cluster);
        //        var cloudCreatures = cluster?.CloudInventories?.SelectMany(x => x.Dinos)?.Where(x => !x.ClassName.Equals("Raft_BP_C"));

        //        if (cloudCreatures == null) return NoRafts;
        //        if (NoRafts == null) return cloudCreatures;
        //        return cloudCreatures.Concat(NoRafts);
        //    }
        //}

        //public SaveState SaveState { get; set; }
        //public ArkTamedCreature[] TamedCreatures { get; set; }
        //public ArkWildCreature[] WildCreatures { get; set; }
        //public ArkSavegameToolkitNet.Domain.ArkTribe[] Tribes { get; set; }
        //public ArkPlayer[] Players { get; set; }
        //public ArkItem[] Items { get; set; }
        //public ArkStructure[] Structures { get; set; }

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

        public ArkServerContext(
            IConfig fullconfig,
            ServerConfigSection config,
            ArkClusterContext clusterContext,
            ISavedState savedState,
            ArkAnonymizeData anonymizeData,
            ILifetimeScope scope)
            : base(
                  config?.SaveFilePath,
                  clusterContext,
                  fullconfig.SavegameExtractionMaxDegreeOfParallelism,
                  true)
        {
            Config = config;
            _clusterContext = clusterContext;
            _scope = scope;
            _saveFileWatcher = _scope.Resolve<IArkSaveFileWatcher>(new TypedParameter(typeof(ArkServerContext), this));
            _savedState = savedState;
            _anonymizeData = anonymizeData;
            Steam = new SteamManager(config);
        }

        public async Task Initialize()
        {
            await Steam.Initialize();
        }

        public bool Update(bool manualUpdate, IConfig fullconfig, ISavegameBackupService savegameBackupService, IProgress<string> progress, CancellationToken ct)
        {
            //backup this savegame
            if (!manualUpdate)
            {
                SavegameBackupResult bresult = null;
                try
                {
                    if (fullconfig.Backups.BackupsEnabled)
                    {
                        bresult = savegameBackupService.CreateBackup(Config, _contextManager?.GetCluster(Config.ClusterKey)?.Config);
                        if (bresult != null && bresult.ArchivePaths != null) progress.Report($@"Server ({Config.Key}): Backup successfull ({(string.Join(", ", bresult.ArchivePaths.Select(x => $@"""{x}""")))})!");
                        else progress.Report($"Server ({Config.Key}): Backup failed...");
                    }
                }
                catch (Exception ex) { Logging.LogException($"Server ({Config.Key}): Backup failed", ex, typeof(ArkServerContext), LogLevel.ERROR, ExceptionLevel.Ignored); }
                BackupCompleted?.Invoke(this, fullconfig.Backups.BackupsEnabled, bresult);
            }


            //todo: temp copy all
            ArkGameDataUpdateResult result = null;
            var st = Stopwatch.StartNew();
            try
            {
                progress.Report($"Server ({Config.Key}): Update started ({DateTime.Now:HH:mm:ss.ffff})");

                result = Update(ct, _savedState.PlayerLastActive.Where(x => x.ServerKey != null && x.ServerKey.Equals(Config.Key, StringComparison.OrdinalIgnoreCase)).Select(x => 
                    new ArkPlayerExternal { Id = x.Id, SteamId = x.SteamId, TribeId = x.TribeId, LastActiveTime = x.LastActiveTime, Name = x.Name, CharacterName = x.CharacterName })
                .ToArray(), _clusterContext != null, fullconfig.AnonymizeWebApiData ? _anonymizeData : null); //update and defer apply new data until cluster is updated

                if (result?.Success == true)
                {
                    progress.Report($"Server ({Config.Key}): Update finished in {st.ElapsedMilliseconds:N0} ms");
                    IsInitialized = true;

                    LastUpdate = DateTime.Now;
                }

                if (result?.Cancelled == true)
                    progress.Report($"Server ({Config.Key}): Update was cancelled after {st.ElapsedMilliseconds:N0} ms");


            }
            catch (Exception ex)
            {
                Logging.LogException($"Failed to update server ({Config.Key})", ex, typeof(ArkServerContext), LogLevel.ERROR, ExceptionLevel.Ignored);
                progress.Report($"Server ({Config.Key}): Update failed after {st.ElapsedMilliseconds:N0} ms");
            }
            finally
            {
                UpdateCompleted?.Invoke(this, result?.Success ?? false, result?.Cancelled ?? false);
                if (result?.Success == true && result?.Cancelled == false && _clusterContext == null) OnGameDataUpdated();
            }

            return result?.Success ?? false;
        }

        public void OnGameDataUpdated()
        {
            GameDataUpdated?.Invoke(this);
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
