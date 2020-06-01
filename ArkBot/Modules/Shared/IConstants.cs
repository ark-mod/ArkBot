namespace ArkBot.Modules.Shared
{
    public interface IConstants
    {
        string ConfigurationHelpTemplatePath { get; }
        string AboutTemplatePath { get; }
        string SavedStateFilePath { get; }
        string ArkServerProcessName { get; }
    }
}