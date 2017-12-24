using System.Collections.Generic;

namespace ArkBot
{
    public interface IConfig
    {
        ArkMultipliersConfigSection ArkMultipliers { get; set; }
        string BotId { get; set; }
        string BotName { get; set; }
        string BotNamespace { get; set; }
        string BotToken { get; set; }
        string BotUrl { get; set; }
        string AppUrl { get; set; }
        string GoogleApiKey { get; set; }
        string SteamApiKey { get; set; }
        string SteamOpenIdRedirectUri { get; set; }
        string SteamOpenIdRelyingServiceListenPrefix { get; set; }
        string TempFileOutputDirPath { get; set; }
        bool DisableDeveloperFetchSaveData { get; set; }
        string MemberRoleName { get; set; }
        DiscordConfigSection Discord { get; set; }
        Dictionary<string, string[]> UserRoles { get; set; }
        Dictionary<string, Dictionary<string, string[]>> AccessControl { get; set; }
        string[] EnabledChannels { get; set; }
        string InfoTopicChannel { get; set; }
        string AnnouncementChannel { get; set; }
        bool BackupsEnabled { get; set; }
        string BackupsDirectoryPath { get; set; }
        bool DiscordBotEnabled { get; set; }
        string WebApiListenPrefix { get; set; }
        string WebAppListenPrefix { get; set; }
        string[] WebAppRedirectListenPrefix { get; set; }
        string PowershellFilePath { get; set; }
        bool UseCompatibilityChangeWatcher { get; set; }
        SslConfigSection Ssl { get; set; }
        int? SavegameExtractionMaxDegreeOfParallelism { get; set; }
        bool AnonymizeWebApiData { get; set; }

        ServerConfigSection[] Servers { get; set; }
        ClusterConfigSection[] Clusters { get; set; }
    }
}