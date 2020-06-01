using ArkBot.Modules.Application.Services.Data;
using System;

namespace ArkBot.Modules.Application
{
    public class ContextUpdatingEventArgs : EventArgs
    {
        public ContextUpdatingEventArgs(bool wasTriggeredBySaveFileChange, SavegameBackupResult savegameBackupResult = null)
        {
            WasTriggeredBySaveFileChange = wasTriggeredBySaveFileChange;
            SavegameBackupResult = savegameBackupResult;
        }

        public bool WasTriggeredBySaveFileChange { get; set; }
        public SavegameBackupResult SavegameBackupResult { get; set; }
    }
}
