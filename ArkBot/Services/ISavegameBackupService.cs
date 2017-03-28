using ArkBot.Services.Data;

namespace ArkBot.Services
{
    public interface ISavegameBackupService
    {
        SavegameBackupResult CreateBackup(string saveFilePath, string clusterSavePath);
    }
}