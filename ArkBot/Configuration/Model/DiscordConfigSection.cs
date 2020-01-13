using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkBot.Configuration;
using ArkBot.Configuration.Validation;
using Discord;
using Microsoft.IdentityModel;
using PropertyChanged;
using Validar;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace ArkBot.Configuration.Model
{
    public class DiscordConfigSection
    {
        public DiscordConfigSection()
        {
            DiscordBotEnabled = true;
            AccessControl = new AccessControlConfigSection();
        }

        public override string ToString() => $"Discord ({(DiscordBotEnabled ? "Enabled" : "Disabled")})";

        [JsonProperty(PropertyName = "discordBotEnabled")]
        [Display(Name = "Discord Bot Enabled", Description = "Option to enable/disable the discord bot component")]
        public bool DiscordBotEnabled { get; set; }

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
        [Display(Name = "Disable Developer Fetch Save Data", Description = "Diable users in \"developer\"-role fetching json or save file data")]
        public bool DisableDeveloperFetchSaveData { get; set; }

        [JsonProperty(PropertyName = "accessControl")]
        [Display(Name = "Access Control", Description = "Per-feature role based access control configuration")]
        public AccessControlConfigSection AccessControl { get; set; }

        [JsonProperty(PropertyName = "steamOpenIdRelyingServiceListenPrefix")]
        [Display(Name = "Steam Open ID Relying Server Listen Prefix", Description = "Http listen prefix for Steam OpenID Relying Party webservice (requires a port that is open to external connections)")]
        public string SteamOpenIdRelyingServiceListenPrefix { get; set; }

        [JsonProperty(PropertyName = "steamOpenIdRedirectUri")]
        [Display(Name = "Steam Open ID Redirect URL", Description = "Publicly accessible url for incoming Steam OpenID Relying Party webservice connections (requires a port that is open to external connections)")]
        public string SteamOpenIdRedirectUri { get; set; }
    }
}
