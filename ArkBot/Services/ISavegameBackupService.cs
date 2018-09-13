using ArkBot.Configuration.Model;
using ArkBot.Services.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace ArkBot.Services
{
    public enum StashResult { Successfull, MoveFailed, SourceMissing, TargetExists }

    public interface ISavegameBackupService
    {
        List<BackupListEntity> GetBackupsList(string[] keys = null, Func<FileInfo, BackupListEntity, bool> filterFunc = null);

        List<BackupListEntity> GetCloudBackupFilesForSteamId(ClusterConfigSection cluster, long steamId);

        StashResult StashCloudSave(ClusterConfigSection cluster, long steamId, string tagName);

        StashResult PopCloudSave(ClusterConfigSection cluster, long steamId, string tagName);

        SavegameBackupResult CreateBackup(ServerConfigSection server, ClusterConfigSection cluster);

        SavegameBackupResult CreateClusterBackupForSteamId(ClusterConfigSection cluster, long steamId);
    }
}