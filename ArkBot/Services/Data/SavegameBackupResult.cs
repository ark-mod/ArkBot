using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Services.Data
{
    public class SavegameBackupResult
    {
        public string[] ArchivePaths { get; set; }
        public string[] FilesInBackup { get; set; }
        public int SaveGameCount { get; set; }
        public int ArkprofileCount { get; set; }
        public int ArktribeCount { get; set; }
        public int ClusterCount { get; set; }
    }
}
