using ArkBot.Ark;
using ArkBot.Database;
using ArkBot.Database.Model;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using ArkBot.Configuration.Model;

namespace ArkBot.Discord
{
    /// <summary>
    /// Abstract some Discord functions
    /// </summary>
    public class DiscordManager
    {
        private DiscordSocketClient _discord;
        private IConfig _config;

        public DiscordManager(DiscordSocketClient discord, IConfig config)
        {
            _discord = discord;
            _config = config;
        }

        public async Task SendTextMessageToChannelNameOnAllServers(string channelName, string mesage)
        {
            var channels = _discord.Guilds.Select(x => x.TextChannels.FirstOrDefault(y => channelName.Equals(y.Name, StringComparison.OrdinalIgnoreCase))).Where(x => x != null);
            foreach (var channel in channels)
            {
                await channel.SendMessageAsync(mesage);
            }
        }

        public async Task EditChannelByNameOnAllServers(string infoTopicChannel, string name = null, string topic = null, int? position = null)
        {
            var channels = _discord.Guilds.Select(x => x.TextChannels.FirstOrDefault(y => _config.Discord.InfoTopicChannel.Equals(y.Name, StringComparison.OrdinalIgnoreCase))).Where(x => x != null);
            foreach (var channel in channels)
            {
                await channel.ModifyAsync(g =>
                {
                    //name, topic, position
                    if (name != null) g.Name = name;
                    if (topic != null) g.Topic = topic;
                    if (position != null) g.Position = position.Value;
                });
            }
        }

        public string GetDiscordUserNameById(ulong id)
        {
            foreach (var server in _discord.Guilds)
            {
                var user = server.GetUser(id);
                if (user != null) return $"{user.Discriminator}";
            }

            return null;
        }
    }
}
