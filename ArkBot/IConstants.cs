namespace ArkBot
{
    public interface IConstants
    {
        string DatabaseConnectionString { get; }
        string OpenidresponsetemplatePath { get; }
        string ConfigurationHelpTemplatePath { get; }
        string AboutTemplatePath { get; }
        string DatabaseFilePath { get; }
        string SavedStateFilePath { get; }
        string ArkServerProcessName { get; }
    }
}