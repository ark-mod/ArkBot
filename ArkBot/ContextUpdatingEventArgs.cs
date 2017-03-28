using ArkBot.Database.Model;
using ArkBot.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
