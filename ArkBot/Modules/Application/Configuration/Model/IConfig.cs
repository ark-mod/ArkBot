namespace ArkBot.Modules.Application.Configuration.Model
{
    public interface IConfig
    {
        void SetupDefaults();

        string SteamApiKey { get; set; }
        string DatabaseConnectionString { get; set; }
        string TempFileOutputDirPath { get; set; }
        WebAppConfigSection WebApp { get; set; }
        DiscordConfigSection Discord { get; set; }
        BackupsConfigSection Backups { get; set; }
        ServersConfigSection Servers { get; set; }
        ClustersConfigSection Clusters { get; set; }
        string PowershellFilePath { get; set; }
        PrometheusConfigSection Prometheus { get; set; }
        AuctionHouseConfigSection AuctionHouse { get; set; }
        bool UseCompatibilityChangeWatcher { get; set; }
        bool AnonymizeWebApiData { get; set; }

        //Test1ConfigSection Test { get; set; }
    }
}