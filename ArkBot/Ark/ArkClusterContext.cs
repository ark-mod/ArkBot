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
    public class ArkClusterContext : IArkUpdateableContext
    {
        public ClusterConfigSection Config { get; set; }

        public event UpdateCompletedEventHandler UpdateCompleted;

        public bool IsInitialized { get; set; }

        public ArkSavegameToolkitNet.Domain.ArkCloudInventory[] CloudInventories { get; set; }

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

        public ArkClusterContext(ClusterConfigSection config)
        {
            Config = config;
        }

        public bool Update(bool manualUpdate, IConfig fullconfig, ISavegameBackupService savegameBackupService, IProgress<string> progress, CancellationToken ct)
        {
            //todo: temp copy all
            var copy = true;
            var success = false;
            var cancelled = false;
            var tmppaths = new List<string>();
            var gid = Guid.NewGuid().ToString();
            var tempFileOutputDirPath = Path.Combine(fullconfig.TempFileOutputDirPath, gid);
            var st = Stopwatch.StartNew();
            try
            {
                progress.Report($"Cluster ({Config.Key}): Update started ({DateTime.Now:HH:mm:ss.ffff})");

                if (copy)
                {
                    //todo: if it exists get a new path
                    if (!Directory.Exists(tempFileOutputDirPath)) Directory.CreateDirectory(tempFileOutputDirPath);
                }

                ArkSavegameToolkitNet.ArkCloudInventory[] cloudInventories = null;
                if (copy)
                {
                    var cloudInventoryPaths = new List<string>();
                    foreach (var ci in Directory.GetFiles(Config.SavePath, "*", SearchOption.TopDirectoryOnly))
                    {
                        var cloudInventoryPath = Path.Combine(tempFileOutputDirPath, Path.GetFileName(ci));
                        cloudInventoryPaths.Add(ci);
                        tmppaths.Add(cloudInventoryPath);
                        File.Copy(ci, cloudInventoryPath);
                    }
                    cloudInventories = cloudInventoryPaths.Select(x => new ArkSavegameToolkitNet.ArkCloudInventory(x)).ToArray();
                }
                else cloudInventories = Directory.GetFiles(Config.SavePath, "*", SearchOption.TopDirectoryOnly).Select(x => new ArkSavegameToolkitNet.ArkCloudInventory(x)).ToArray();

                CloudInventories = cloudInventories.Where(x => x.InventoryData != null).Select(x => x.InventoryData.AsCloudInventory(x.SteamId, SaveState.FromSaveTime(x.SaveTime), x.InventoryDinoData)).ToArray();

                progress.Report($"Cluster ({Config.Key}): Update finished in {st.ElapsedMilliseconds:N0} ms");
                IsInitialized = true;

                LastUpdate = DateTime.Now;
                success = true;
            }
            catch (OperationCanceledException)
            {
                progress.Report($"Cluster ({Config.Key}): Update was cancelled after {st.ElapsedMilliseconds:N0} ms");
                cancelled = true;
            }
            catch (Exception ex)
            {
                Logging.LogException($"Failed to update cluster ({Config.Key})", ex, this.GetType(), LogLevel.ERROR, ExceptionLevel.Ignored);
                progress.Report($"Cluster ({Config.Key}): Update failed after {st.ElapsedMilliseconds:N0} ms");
            }
            finally
            {
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
    }
}
