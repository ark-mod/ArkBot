using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkBot.Helpers;
using ArkBot.Services.Data;
using ArkBot.Extensions;
using ArkBot.Configuration.Model;

namespace ArkBot.Services
{
    public class SavegameBackupService : ISavegameBackupService
    {
        private IConfig _config;
        
        public SavegameBackupService(IConfig config)
        {
            _config = config;
        }

        /// <param name="keys">Array of server- and/or cluster key(s)</param>
        /// <returns></returns>
        public List<BackupListEntity> GetBackupsList(string[] keys = null, Func<FileInfo, BackupListEntity, bool> filterFunc = null)
        {
            var result = new List<BackupListEntity>();

            foreach (var key in keys ?? new string[] { null })
            {
                var backupDirPath = key == null ? _config.Backups.BackupsDirectoryPath : Path.Combine(_config.Backups.BackupsDirectoryPath, key);
                var backupDir = new DirectoryInfo(backupDirPath);
                var files = backupDir.GetFiles("*.zip", SearchOption.AllDirectories);
                if (files == null) return result;

                foreach (var file in files)
                {
                    var a = file.FullName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    var b = _config.Backups.BackupsDirectoryPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    var path = Path.Combine(a.Merge(b, (_a, _b) => new { a = _a, b = _b }).SkipWhile(x => x.a.Equals(x.b, StringComparison.OrdinalIgnoreCase)).Select(x => x.a).ToArray());

                    var entry = new BackupListEntity
                    {
                        Path = path,
                        FullPath = file.FullName,
                        ByteSize = file.Length,
                        DateModified = file.LastWriteTime,
                        LazyFiles = new Lazy<string[]>(() => FileHelper.GetZipFileContents(file.FullName))
                    };
                    if (filterFunc != null && !filterFunc(file, entry)) continue;

                    result.Add(entry);
                }
            }

            return result;
        }

        public List<BackupListEntity> GetCloudBackupFilesForSteamId(ClusterConfigSection cluster, long steamId)
        {
            var result = new List<BackupListEntity>();

            var backupDirPath = cluster.SavePath;
            var backupDir = new DirectoryInfo(backupDirPath);
            var files = backupDir.GetFiles($"{steamId}.*", SearchOption.AllDirectories).Where(x => !x.Name.Equals(steamId.ToString())).ToArray();
            if (files == null) return result;

            foreach (var file in files)
            {
                var a = file.FullName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var b = backupDirPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var path = Path.Combine(a.Merge(b, (_a, _b) => new { a = _a, b = _b }).SkipWhile(x => x.a.Equals(x.b, StringComparison.OrdinalIgnoreCase)).Select(x => x.a).ToArray());

                var entry = new FromServerBackupListEntity
                {
                    Path = Path.Combine("current", path),
                    FullPath = file.FullName,
                    ByteSize = file.Length,
                    DateModified = DateTime.Now,
                    Files = new[] { path }
                };

                result.Add(entry);
            }

            return result;
        }

        public StashResult StashCloudSave(ClusterConfigSection cluster, long steamId, string tagName)
        {
            try
            {
                var sourcePath = Path.Combine(cluster.SavePath, $"{steamId}");
                var targetPath = Path.Combine(cluster.SavePath, $"{steamId}.stash_{tagName}");
                if (!File.Exists(sourcePath)) return StashResult.SourceMissing;
                if (File.Exists(targetPath)) return StashResult.TargetExists;

                File.Move(sourcePath, targetPath);
            }
            catch
            {
                /*ignore exceptions*/
                return StashResult.MoveFailed;
            }

            return StashResult.Successfull;
        }

        public StashResult PopCloudSave(ClusterConfigSection cluster, long steamId, string tagName)
        {
            try
            {
                var sourcePath = Path.Combine(cluster.SavePath, $"{steamId}.stash_{tagName}");
                var targetPath = Path.Combine(cluster.SavePath, $"{steamId}");
                if (!File.Exists(sourcePath)) return StashResult.SourceMissing;
                if (File.Exists(targetPath)) return StashResult.TargetExists;

                File.Move(sourcePath, targetPath);
            }
            catch
            {
                /*ignore exceptions*/
                return StashResult.MoveFailed;
            }

            return StashResult.Successfull;
        }

        public SavegameBackupResult CreateBackup(ServerConfigSection server, ClusterConfigSection cluster)
        {
            if (!File.Exists(server.SaveFilePath))
            {
                Logging.Log($@"Savegame backup was requested but there are no files to backup (saveFilePath: ""{server.SaveFilePath}"", clusterSavePath: ""{cluster?.SavePath ?? "-"}"")", GetType(), LogLevel.DEBUG);
                return null;
            }

            var dir = Path.GetDirectoryName(server.SaveFilePath);
            string[] arkprofiles = null;
            string[] arktribes = null;
            string[] clusters = null;
            var files = new[]
            {
                    new Tuple<string, string, string[]>("", "", new [] { server.SaveFilePath }),
                    Directory.Exists(dir) ? new Tuple<string, string, string[]>("", "", arkprofiles = Directory.GetFiles(dir, "*.arkprofile", SearchOption.TopDirectoryOnly)) : null,
                    Directory.Exists(dir) ? new Tuple<string, string, string[]>("", "", arktribes = Directory.GetFiles(dir, "*.arktribe", SearchOption.TopDirectoryOnly)) : null,
                    cluster != null ? new Tuple<string, string, string[]>(cluster.SavePath, "cluster", clusters = Directory.GetFiles(cluster.SavePath, "*", SearchOption.AllDirectories)) : null
                }.Where(x => x != null && x.Item2 != null).ToArray();

            var backupDir = Path.Combine(_config.Backups.BackupsDirectoryPath, server.Key, DateTime.Now.ToString("yyyy-MM"));

            if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);

            var path = Path.Combine(backupDir, "save_" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
            string[] results = null;
            try
            {
                results = FileHelper.CreateDotNetZipArchive(files, path);
            }
            catch (Exception ex)
            {
                Logging.LogException("Failed to create savegame backup archive", ex, GetType(), LogLevel.ERROR, ExceptionLevel.Ignored);
                return null;
            }

            return new SavegameBackupResult
            {
                ArchivePaths = results,
                FilesInBackup = files.SelectMany(x => x.Item3).ToArray(),
                SaveGameCount = 1,
                ArkprofileCount = arkprofiles?.Length ?? 0,
                ArktribeCount = arktribes?.Length ?? 0,
                ClusterCount = clusters?.Length ?? 0
            };
        }

        public SavegameBackupResult CreateClusterBackupForSteamId(ClusterConfigSection cluster, long steamId)
        {
            if (cluster == null || string.IsNullOrEmpty(cluster.SavePath) || !Directory.Exists(cluster.SavePath)) return null;

            string[] clusters = null;
            var files = new[] { new Tuple<string, string, string[]>(cluster.SavePath, "cluster", clusters = Directory.GetFiles(cluster.SavePath, $"{steamId}*", SearchOption.AllDirectories))}.ToArray();

            if (!(clusters?.Length > 0)) return null;

            var backupDir = Path.Combine(_config.Backups.BackupsDirectoryPath, cluster.Key, DateTime.Now.ToString("yyyy-MM"));

            if (!Directory.Exists(backupDir)) Directory.CreateDirectory(backupDir);

            var path = Path.Combine(backupDir, $"cluster_{steamId}_" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
            string[] results = null;
            try
            {
                results = FileHelper.CreateDotNetZipArchive(files, path);
            }
            catch (Exception ex)
            {
                Logging.LogException($"Failed to create cluster backup archive for steamid {steamId}", ex, GetType(), LogLevel.ERROR, ExceptionLevel.Ignored);
                return null;
            }

            return new SavegameBackupResult
            {
                ArchivePaths = results,
                FilesInBackup = files.SelectMany(x => x.Item3).ToArray(),
                SaveGameCount = 0,
                ArkprofileCount = 0,
                ArktribeCount = 0,
                ClusterCount = clusters.Length
            };
        }
    }
}
