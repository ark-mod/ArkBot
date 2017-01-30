using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class ArkSaveFileChangedEventArgs : EventArgs
    {
        public string SaveFileName { get; set; }
    }
}
