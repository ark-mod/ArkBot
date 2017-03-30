using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Services.Data
{
    public class BackupListEntity
    {
        public string Path { get; set; }
        public long ByteSize { get; set; }
        public DateTime DateModified { get; set; }
        public string[] Files { get;  set; }
    }
}
