using System;
using System.Threading.Tasks;

namespace ArkBot.OpenID
{
    public interface IBarebonesSteamOpenId: IDisposable
    {
        event BarebonesSteamOpenId.SteamOpenIdCallbackEventHandler SteamOpenIdCallback;

        Task<SteamOpenIdState> LinkWithSteamTaskAsync(ulong discordUserId);
    }
}