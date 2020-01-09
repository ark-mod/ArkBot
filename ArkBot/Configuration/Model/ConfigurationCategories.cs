namespace ArkBot.Configuration.Model
{
    internal static class ConfigurationCategory
    {
        /// <summary>
        /// Settings that must be changed (environment specific)
        /// </summary>
        internal const string Required = "Required";
        /// <summary>
        /// Settings that are either optional or may be left at default
        /// </summary>
        internal const string Optional = "Optional";

        /// <summary>
        /// Optional settings for advanced configurations
        /// </summary>
        internal const string Advanced = "Advanced";

        /// <summary>
        /// Optional setting for debugging, logging etc.
        /// </summary>
        internal const string Debug = "Debug";
    }
}
