using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.OpenID
{
    public class SteamOpenIdState
    {
        //public string Realm { get; set; }
        public DateTime When { get; set; }
        public Uri ReturnTo { get; set; }
        public string Identity { get; set; }
        public string ClaimedId { get; set; }
        public string Authority { get; set; }
        public string StartUrl { get; set; }
        public ulong DiscordUserId { get; set; }
    }
}
