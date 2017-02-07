using BrightstarDB.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Database
{
    [Entity]
    public interface IUser
    {
        [Identifier("adb:users/", KeyProperties = new [] { nameof(DiscordId) })]
        string Id { get; }
        ulong DiscordId { get; set; }
        ulong SteamId { get; set; }
        string RealName { get; set; }
        string SteamDisplayName { get; set; }
    }
}
