using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkBot.Helpers;
using ArkBot.Services.Data;
using ArkBot.Extensions;

namespace ArkBot.Services
{
    public class SavegameBackupService : ISavegameBackupService
    {
        private IConfig _config;
        
        public SavegameBackupService(IConfig config)
        {
            _config = config;
        }

        public IList<BackupListEntity> GetBackupsList(string serverKey = null, bool includeContents = false)
        {
            var result = new List<BackupListEntity>();

            var backupDirPath = serverKey == null ? _config.BackupsDirectoryPath : Path.Combine(_config.BackupsDirectoryPath, serverKey);
            var backupDir = new DirectoryInfo(backupDirPath);
            var files = backupDir.GetFiles("*.zip", SearchOption.AllDirectories);
            if (files == null) return result;

            foreach(var file in files)
            {
                var a = file.FullName.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var b = backupDirPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var path = Path.Combine(a.Merge(b, (_a, _b) => new { a = _a, b = _b }).SkipWhile(x => x.a.Equals(x.b, StringComparison.OrdinalIgnoreCase)).Select(x => x.a).ToArray());

                result.Add(new BackupListEntity
                {
                    Path = path,
                    ByteSize = file.Length,
                    DateModified = file.LastWriteTime,
                    Files = includeContents ? FileHelper.GetZipFileContents(file.FullName) : null
                });
            }

            return result;
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

            var backupDir = Path.Combine(_config.BackupsDirectoryPath, server.Key, DateTime.Now.ToString("yyyy-MM"));

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
    }
}
