using Discord;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArkBot.Modules.Application.Configuration.Model
{
    public class DiscordConfigSection
    {
        public DiscordConfigSection()
        {
            Enabled = true;
            AccessControl = new AccessControlConfigSection();
        }

        public override string ToString() => $"Discord ({(Enabled ? "Enabled" : "Disabled")})";

        [JsonProperty(PropertyName = "enabled")]
        [Display(Name = "Enabled", Description = "Option to enable/disable the discord bot component")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "botToken")]
        [Display(Name = "Bot Token", Description = "Bot authentication token from https://discordapp.com/developers")]
        public string BotToken { get; set; }

        [JsonProperty(PropertyName = "enabledChannels")]
        [Display(Name = "Enabled Channels", Description = "A list of channels where the bot will listen to and answer commands")]
        public List<string> EnabledChannels { get; set; }

        [JsonProperty(PropertyName = "infoTopicChannel")]
        [Display(Name = "Info Topic Channel", Description = "Channel where topic is set to display information about last update, next update and how to use bot commands")]
        public string InfoTopicChannel { get; set; }

        [JsonProperty(PropertyName = "announcementChannel")]
        [Display(Name = "Announcement Channel", Description = "Channel where announcements are made (votes etc.)")]
        public string AnnouncementChannel { get; set; }

        [JsonProperty(PropertyName = "memberRoleName")]
        [Display(Name = "Member Role Name", Description = "The name of the member role in Discord")]
        public string MemberRoleName { get; set; }

        [JsonProperty(PropertyName = "disableDeveloperFetchSaveData")]
        [Display(Name = "Disable Developer Fetch Save Data", Description = "Disable users in \"developer\"-role fetching json or save file data")]
        public bool DisableDeveloperFetchSaveData { get; set; }

        [JsonProperty(PropertyName = "accessControl")]
        [Display(Name = "Access Control", Description = "Per-feature role based access control configuration")]
        public AccessControlConfigSection AccessControl { get; set; }

        [JsonProperty(PropertyName = "logLevel")]
        [Display(Name = "Log Level", Description = "Log level")]
        //[Category(ConfigurationCategory.Debug)]
        //[PropertyOrder(0)]
        public LogSeverity LogLevel { get; set; }
    }
}
