using ArkBot.Services.Data;
using System.Collections.Generic;

namespace ArkBot.Services
{
    public interface ISavegameBackupService
    {
        IList<BackupListEntity> GetBackupsList(bool includeContents = false);
        SavegameBackupResult CreateBackup(string saveFilePath, string clusterSavePath);
    }
}