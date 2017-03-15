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
        string ClusterSavePath { get; set; }
        bool Debug { get; set; }
        bool DebugNoExtract { get; set; }
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
    }
}