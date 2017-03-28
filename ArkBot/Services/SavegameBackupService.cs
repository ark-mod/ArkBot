using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkBot.Helpers;
using ArkBot.Services.Data;

namespace ArkBot.Services
{
    public class SavegameBackupService : ISavegameBackupService
    {
        private IConfig _config;
        
        public SavegameBackupService(IConfig config)
        {
            _config = config;
        }

        public SavegameBackupResult CreateBackup(string saveFilePath, string clusterSavePath)
        {
            if (!File.Exists(saveFilePath))
            {
                Logging.Log($@"Savegame backup was requested but there are no files to backup (saveFilePath: ""{saveFilePath}"", clusterSavePath: ""{clusterSavePath}"")", GetType(), LogLevel.DEBUG);
                return null;
            }

            var dir = Path.GetDirectoryName(saveFilePath);
            string[] arkprofiles = null;
            string[] arktribes = null;
            string[] clusters = null;
            var files = new[]
            {
                    new Tuple<string, string, string[]>("", "", new [] { saveFilePath }),
                    Directory.Exists(dir) ? new Tuple<string, string, string[]>("", "", arkprofiles = Directory.GetFiles(dir, "*.arkprofile", SearchOption.TopDirectoryOnly)) : null,
                    Directory.Exists(dir) ? new Tuple<string, string, string[]>("", "", arktribes = Directory.GetFiles(dir, "*.arktribe", SearchOption.TopDirectoryOnly)) : null,
                    new Tuple<string, string, string[]>(clusterSavePath, "cluster", clusters = Directory.GetFiles(clusterSavePath, "*", SearchOption.AllDirectories))
                }.Where(x => x != null && x.Item2 != null).ToArray();

            if (!Directory.Exists(_config.BackupsDirectoryPath)) Directory.CreateDirectory(_config.BackupsDirectoryPath);

            var path = Path.Combine(_config.BackupsDirectoryPath, "save_" + DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff") + ".zip");
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
