using ArkBot.Services.Data;
using System;

namespace ArkBot
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
