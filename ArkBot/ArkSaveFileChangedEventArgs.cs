using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class ArkSaveFileChangedEventArgs : EventArgs
    {
        public string PathToLoad { get; set; }
        public string SaveFilePath { get; set; }
        public string ClusterPath { get; set; }
    }
}
