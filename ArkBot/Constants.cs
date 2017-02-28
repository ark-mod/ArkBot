using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class Constants : IConstants
    {
        public string DatabaseConnectionString => "type=embedded;storesdirectory=.\\Database;storename=Default";
        public string OpenidresponsetemplatePath => @"Resources\openidresponse.html";
        public string DatabaseDirectoryPath => ".\\Database";
        public string SavedStateFilePath => "savedstate.json";
        //todo: move this to config
        public string ServerIp => "85.227.28.132";
        public int ServerPort => 27003;
    }
}
