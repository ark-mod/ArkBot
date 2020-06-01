namespace ArkBot.Modules.Shared
{
    public class Constants : IConstants
    {
        public string ConfigurationHelpTemplatePath => @"Resources\configurationHelp.html";
        public string AboutTemplatePath => @"Resources\about.html";
        public string SavedStateFilePath => "savedstate.json";
        public string ArkServerProcessName => "ShooterGameServer";
    }
}
