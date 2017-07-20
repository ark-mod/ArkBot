using ArkBot.Services.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace ArkBot.Services
{
    public interface ISavegameBackupService
    {
        IList<BackupListEntity> GetBackupsList(string[] serverKeys = null, bool includeContents = false, Func<FileInfo, BackupListEntity, bool> filterFunc = null);

        SavegameBackupResult CreateBackup(ServerConfigSection server, ClusterConfigSection cluster);
    }
}