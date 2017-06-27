namespace ArkBot
{
    public interface IConfig
    {
        ArkMultipliersConfigSection ArkMultipliers { get; set; }
        string ArktoolsExecutablePath { get; set; }
        string BotId { get; set; }
        string BotName { get; set; }
        string BotNamespace { get; set; }
        string BotToken { get; set; }
        string BotUrl { get; set; }
        string AppUrl { get; set; }
        string ClusterSavePath { get; set; }
        //bool Debug { get; set; }
        //string DebugSaveFilePath { get; set; }
        //string DebugClusterSavePath { get; set; }
        //string DebugJsonOutputDirPath { get; set; }
        //bool DebugNoExtract { get; set; }
        string GoogleApiKey { get; set; }
        string JsonOutputDirPath { get; set; }
        string SaveFilePath { get; set; }
        string SteamApiKey { get; set; }
        string SteamOpenIdRedirectUri { get; set; }
        string SteamOpenIdRelyingServiceListenPrefix { get; set; }
        string TempFileOutputDirPath { get; set; }
        bool DisableDeveloperFetchSaveData { get; set; }
        string AdminRoleName { get; set; }
        string DeveloperRoleName { get; set; }
        string MemberRoleName { get; set; }
        string ServerKey { get; set; }
        string ServerIp { get; set; }
        int ServerPort { get; set; }
        string[] EnabledChannels { get; set; }
        string InfoTopicChannel { get; set; }
        string AnnouncementChannel { get; set; }
        bool BackupsEnabled { get; set; }
        string BackupsDirectoryPath { get; set; }
        bool DiscordBotEnabled { get; set; }
        string WebApiListenPrefix { get; set; }
        string WebAppListenPrefix { get; set; }
        string PowershellFilePath { get; set; }
        bool UseCompatibilityChangeWatcher { get; set; }

        ServerConfigSection[] Servers { get; set; }
        ClusterConfigSection[] Clusters { get; set; }
    }
}