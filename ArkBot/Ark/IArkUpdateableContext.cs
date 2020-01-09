using ArkBot.Configuration.Model;
using ArkBot.Services;
using System;
using System.Threading;

namespace ArkBot.Ark
{
    public interface IArkUpdateableContext
    {
        bool Update(bool manualUpdate, IConfig fullconfig, ISavegameBackupService savegameBackupService, IProgress<string> progress, CancellationToken ct);
    }
}
