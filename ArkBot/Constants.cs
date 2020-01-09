using System;
using System.IO;

namespace ArkBot
{
    public class Constants : IConstants
    {
        //public string DatabaseFilePath
        //{
        //    get
        //    {
        //        var dataDir = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
        //        if (string.IsNullOrEmpty(dataDir)) dataDir = AppDomain.CurrentDomain.BaseDirectory;
        //        var realPath = _databaseFilePath?.StartsWith("|datadirectory|", StringComparison.OrdinalIgnoreCase) == true ?
        //            Path.Combine(dataDir, _databaseFilePath.Substring("|datadirectory|".Length))
        //            : _databaseFilePath;
        //        return realPath;
        //    }
        //}

        //private string _databaseFilePath => "|DataDirectory|Database\\Database.sdf";
        public string DatabaseConnectionString => @"Server=(localdb)\mssqllocaldb;Database=ArkBot;Integrated Security=True";
        public string OpenidresponsetemplatePath => @"Resources\openidresponse.html";
        public string ConfigurationHelpTemplatePath => @"Resources\configurationHelp.html";
        public string AboutTemplatePath => @"Resources\about.html";
        public string SavedStateFilePath => "savedstate.json";
        public string ArkServerProcessName => "ShooterGameServer";
    }
}
