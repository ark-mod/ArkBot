using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Extensions
{
    public static class DiscordChannelExtensions
    {
        public static async Task<Message> SendMessageDirectedAt(this Channel channel, ulong discordUserId, string text)
        {
            return await channel.SendMessage(channel.GetMessageDirectedAtText(discordUserId, text));
        }

        public static string GetMessageDirectedAtText(this Channel channel, ulong discordUserId, string text)
        {
            if (channel.IsPrivate) return text.FirstCharToUpper();
            else return $"<@{discordUserId}>, {text}";
        }
    }
}
