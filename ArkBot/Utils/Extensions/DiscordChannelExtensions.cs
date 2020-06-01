using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace ArkBot.Utils.Extensions
{
    public static class DiscordChannelExtensions
    {
        public static async Task<IUserMessage> SendMessageDirectedAt(this IMessageChannel channel, ulong discordUserId, string text)
        {
            return await ((ISocketMessageChannel)channel).SendMessageAsync(channel.GetMessageDirectedAtText(discordUserId, text));
        }

        public static string GetMessageDirectedAtText(this IMessageChannel channel, ulong discordUserId, string text)
        {
            if (channel is SocketDMChannel) return text.FirstCharToUpper();
            else return $"<@{discordUserId}>, {text}";
        }
    }
}
