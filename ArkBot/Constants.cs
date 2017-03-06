using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class Constants : IConstants
    {
        public string DatabaseFilePath
        {
            get
            {
                var dataDir = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                if (string.IsNullOrEmpty(dataDir)) dataDir = AppDomain.CurrentDomain.BaseDirectory;
                var realPath = _databaseFilePath?.StartsWith("|datadirectory|", StringComparison.OrdinalIgnoreCase) == true ?
                    Path.Combine(dataDir, _databaseFilePath.Substring("|datadirectory|".Length))
                    : _databaseFilePath;
                return realPath;
            }
        }

        private string _databaseFilePath => "|DataDirectory|Database\\Database.sdf";
        public string DatabaseConnectionString => $"Data Source={_databaseFilePath};Max Database Size=4091";
        public string OpenidresponsetemplatePath => @"Resources\openidresponse.html";
        public string SavedStateFilePath => "savedstate.json";
        //todo: move this to config
        public string ServerIp => "85.227.28.132";
        public int ServerPort => 27003;
    }
}
