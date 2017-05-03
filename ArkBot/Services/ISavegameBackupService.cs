using ArkBot.Services.Data;
using System.Collections.Generic;

namespace ArkBot.Services
{
    public interface ISavegameBackupService
    {
        IList<BackupListEntity> GetBackupsList(string serverKey = null, bool includeContents = false);
        SavegameBackupResult CreateBackup(ServerConfigSection server, ClusterConfigSection cluster);
    }
}