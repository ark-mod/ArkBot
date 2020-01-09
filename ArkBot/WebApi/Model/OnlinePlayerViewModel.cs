namespace ArkBot.WebApi.Model
{
    public class OnlinePlayerViewModel
    { 
        public string SteamName { get; set; }
        public string CharacterName { get; set; }
        public string TribeName { get; set; }
        public string DiscordName { get; set; }
        public string TimeOnline { get; set; }
        public int TimeOnlineSeconds { get; set; }
    }
}
