namespace ArkBot
{
    public interface IConstants
    {
        string DatabaseConnectionString { get; }
        string OpenidresponsetemplatePath { get; }
        string DatabaseDirectoryPath { get; }
        string SavedStateFilePath { get; }
        string ServerIp { get; }
        int ServerPort { get; }
    }
}