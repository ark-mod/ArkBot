using System;

namespace ArkBot.Modules.Application.Configuration
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ConfigurationHelpAttribute : Attribute
    {
        public string Remarks { get; set; }
        public string Instructions { get; set; }
        public string Example { get; set; }

        public ConfigurationHelpAttribute(string[] remarks = null, string[] instructions = null)
        {
            if (remarks != null) Remarks = string.Join(Environment.NewLine, remarks);
            if (instructions != null) Instructions = string.Join(Environment.NewLine, instructions);
        }
    }
}
