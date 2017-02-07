using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.OpenID
{
    public class SteamOpenIdCallbackEventArgs : EventArgs
    {
        public ulong DiscordUserId { get; set; }
        public ulong SteamId { get; set; }
        public bool Successful { get; set; }
    }
}
