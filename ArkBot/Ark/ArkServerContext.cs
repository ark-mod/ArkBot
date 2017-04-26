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
    public class ArkServerContext
    {
        public ServerConfigSection Config { get; set; }
        public IProgress<string> Progress { get; private set; }

        private IArkSaveFileWatcher _saveFileWatcher;

        private static ConcurrentQueueUnique<ArkServerContext> _backingUpdateQueue;
        private static BlockingCollection<ArkServerContext> _updateQueue;
        private static Task _updateManager;
        private static CancellationTokenSource _ctsCurrentUpdate;
        private static ArkServerContext _serverContextCurrentUpdate;

        public IEnumerable<ArkTamedCreature> NoRafts => TamedCreatures?.Where(x => !x.ClassName.Equals("Raft_BP_C"));

        public ArkTamedCreature[] TamedCreatures { get; set; }
        public ArkWildCreature[] WildCreatures { get; set; }
        public ArkSavegameToolkitNet.Domain.ArkTribe[] Tribes { get; set; }
        public ArkPlayer[] Players { get; set; }
        public ArkItem[] Items { get; set; }
        public ArkStructure[] Structures { get; set; }

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

        static ArkServerContext()
        {
            _backingUpdateQueue = new ConcurrentQueueUnique<ArkServerContext>();
            _backingUpdateQueue.ItemAdded += _backingUpdateQueue_ItemAdded;
            _updateQueue = new BlockingCollection<ArkServerContext>();
            _updateManager = Task.Factory.StartNew(_updateManagerRun, TaskCreationOptions.LongRunning);
        }

        private static void _updateManagerRun()
        {
            while (!_updateQueue.IsCompleted)
            {
                ArkServerContext context = null;
                try
                {
                    context = _updateQueue.Take();
                }
                catch (InvalidOperationException) { }

                if (context != null)
                {
                    _ctsCurrentUpdate = new CancellationTokenSource();
                    _serverContextCurrentUpdate = context;
                    context.Update(_ctsCurrentUpdate.Token);
                }
            }
        }

        private static void _backingUpdateQueue_ItemAdded(object sender, ArkServerContext item)
        {
            // cancel previous update of same server context if a new update request was queued
            if (item.Equals(_serverContextCurrentUpdate) && !_ctsCurrentUpdate.IsCancellationRequested) _ctsCurrentUpdate.Cancel();
        }

        public ArkServerContext(ServerConfigSection config, IProgress<string> progress)
        {
            Config = config;
            Progress = progress;

            _saveFileWatcher = new ArkSaveFileWatcher(Config.SaveFilePath);
            _saveFileWatcher.Changed += _saveFileWatcher_Changed;
        }

        private void _saveFileWatcher_Changed(object sender, ArkSaveFileChangedEventArgs e)
        {
            Progress.Report($"Server ({Config.Key}): Update queued by watcher ({DateTime.Now:HH:mm:ss.ffff})");
            _updateQueue.Add(this);
        }

        public void QueueManualUpdate()
        {
            Progress.Report($"Server ({Config.Key}): Update queued manually ({DateTime.Now:HH:mm:ss.ffff})");
            _updateQueue.Add(this);
        }

        private bool Update(CancellationToken ct)
        {
            var success = false;
            var st = Stopwatch.StartNew();
            try
            {
                Progress.Report($"Server ({Config.Key}): Update started ({DateTime.Now:HH:mm:ss.ffff})");

                var save = new ArkSavegame(Config.SaveFilePath);
                save.LoadEverything();
                ct.ThrowIfCancellationRequested();

                var directoryPath = Path.GetDirectoryName(Config.SaveFilePath);
                var tribes = Directory.GetFiles(directoryPath, "*.arktribe", SearchOption.TopDirectoryOnly).Select(x => new ArkSavegameToolkitNet.ArkTribe(x)).ToArray();
                ct.ThrowIfCancellationRequested();

                var profiles = Directory.GetFiles(directoryPath, "*.arkprofile", SearchOption.TopDirectoryOnly).Select(x => new ArkProfile(x)).ToArray();
                ct.ThrowIfCancellationRequested();

                var _myCharacterStatusComponent = ArkName.Create("MyCharacterStatusComponent");
                var statusComponents = save.Objects.Where(x => x.IsDinoStatusComponent).ToDictionary(x => x.Index, x => x);
                var tamed = save.Objects.Where(x => x.IsTamedCreature).Select(x =>
                {
                    var status = statusComponents[x.GetPropertyValue<ObjectReference>(_myCharacterStatusComponent).ObjectId];
                    return x.AsTamedCreature(status, null, save.SaveState);
                }).ToArray();
                var wild = save.Objects.Where(x => x.IsWildCreature).Select(x =>
                {
                    var status = statusComponents[x.GetPropertyValue<ObjectReference>(_myCharacterStatusComponent).ObjectId];
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
                    return x.Profile.AsPlayer(player, save.SaveState);
                }).ToArray();

                TamedCreatures = tamed;
                WildCreatures = wild;
                Players = players;
                Tribes = tribes.Select(x => x.Tribe.AsTribe()).ToArray();
                Items = save.Objects.Where(x => x.IsItem).Select(x => x.AsItem()).ToArray();
                Structures = save.Objects.Where(x => x.IsStructure).Select(x => x.AsStructure(save.SaveState)).ToArray();

                Progress.Report($"Server ({Config.Key}): Update finished in {st.ElapsedMilliseconds:N0} ms");
                LastUpdate = DateTime.Now;
                success = true;
            }
            catch (TaskCanceledException)
            {
                Progress.Report($"Server ({Config.Key}): Update was cancelled after {st.ElapsedMilliseconds:N0} ms");
            }
            catch (Exception ex)
            {
                Logging.LogException($"Failed to update server ({Config.Key})", ex, typeof(ArkServerContext), LogLevel.ERROR, ExceptionLevel.Ignored);
                Progress.Report($"Server ({Config.Key}): Update failed after {st.ElapsedMilliseconds:N0} ms");
            }

            GC.Collect();

            return success;
        }
    }
}
