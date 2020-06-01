using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.Application.Services;
using System;
using System.Threading;

namespace ArkBot.Modules.Application
{
    public interface IArkUpdateableContext
    {
        bool Update(bool manualUpdate, IConfig fullconfig, ISavegameBackupService savegameBackupService, IProgress<string> progress, CancellationToken ct);
    }
}
