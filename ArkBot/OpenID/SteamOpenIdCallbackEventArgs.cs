using System;

namespace ArkBot.OpenID
{
    public class SteamOpenIdCallbackEventArgs : EventArgs
    {
        public ulong DiscordUserId { get; set; }
        public ulong SteamId { get; set; }
        public bool Successful { get; set; }
    }
}
