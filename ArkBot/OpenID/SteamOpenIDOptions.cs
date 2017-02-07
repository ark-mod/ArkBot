using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.OpenID
{
    public class SteamOpenIdOptions
    {
        public string RedirectUri { get; set; }
        public string[] ListenPrefixes { get; set; }
    }
}
