using ArkBot.Configuration.Model;
using ArkBot.Services;
using ArkSavegameToolkitNet;
using ArkSavegameToolkitNet.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Ark
{
    public class ArkClusterContext : ArkClusterData, IArkUpdateableContext
    {
        public ClusterConfigSection Config { get; private set; }

        private ArkAnonymizeData _anonymizeData;
        internal ArkContextManager _contextManager;

        public event GameDataUpdatedEventHandler GameDataUpdated;
        public event UpdateCompletedEventHandler UpdateCompleted;

        public bool IsInitialized { get; set; }

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

        public ArkClusterContext(ClusterConfigSection config, ArkAnonymizeData anonymizeData) : base(config.SavePath, true)
        {
            Config = config;
            _anonymizeData = anonymizeData;
        }

        public bool Update(bool manualUpdate, IConfig fullconfig, ISavegameBackupService savegameBackupService, IProgress<string> progress, CancellationToken ct)
        {
            ArkClusterDataUpdateResult result = null;
            var st = Stopwatch.StartNew();
            try
            {
                progress.Report($"Cluster ({Config.Key}): Update started ({DateTime.Now:HH:mm:ss.ffff})");

                result = Update(ct, false, fullconfig.AnonymizeWebApiData ? _anonymizeData : null);

                if (result?.Success == true)
                {
                    progress.Report($"Cluster ({Config.Key}): Update finished in {st.ElapsedMilliseconds:N0} ms");
                    IsInitialized = true;

                    LastUpdate = DateTime.Now;
                }

                if (result?.Cancelled == true)
                    progress.Report($"Cluster ({Config.Key}): Update was cancelled after {result.Elapsed.TotalMilliseconds:N0} ms");
            }
            catch (Exception ex)
            {
                Logging.LogException($"Failed to update cluster ({Config.Key})", ex, this.GetType(), LogLevel.ERROR, ExceptionLevel.Ignored);
                progress.Report($"Cluster ({Config.Key}): Update failed after {st.ElapsedMilliseconds:N0} ms");
            }
            finally
            {
                try
                {
                    var servers = _contextManager.GetServersInCluster(Config.Key);
                    if (servers != null)
                    {
                        foreach (var serverContext in servers)
                        {
                            var success = serverContext.ApplyPreviousUpdate();
                            if (success == true) serverContext.OnGameDataUpdated();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogException($"Failed to apply updates to servers in cluster ({Config.Key})", ex, this.GetType(), LogLevel.ERROR, ExceptionLevel.Ignored);
                    progress.Report($"Cluster ({Config.Key}): Failed to apply updates to servers in cluster");
                }

                UpdateCompleted?.Invoke(this, result?.Success ?? false, result?.Cancelled ?? false);
            }

            return result?.Success ?? false;
        }
    }
}
