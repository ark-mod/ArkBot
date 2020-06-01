using System;

namespace ArkBot.Modules.Application
{
    public class ArkSaveFileChangedEventArgs : EventArgs
    {
        public string PathToLoad { get; set; }
        public string SaveFilePath { get; set; }
        public string ClusterPath { get; set; }
    }
}
