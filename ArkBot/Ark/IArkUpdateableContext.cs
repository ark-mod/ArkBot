using ArkBot.Configuration.Model;
using ArkBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Ark
{
    public interface IArkUpdateableContext
    {
        bool Update(bool manualUpdate, IConfig fullconfig, ISavegameBackupService savegameBackupService, IProgress<string> progress, CancellationToken ct);
    }
}
