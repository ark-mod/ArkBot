using ArkBot.Ark;
using ArkBot.Database;
using ArkBot.Database.Model;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Discord
{
    /// <summary>
    /// Abstract some Discord functions
    /// </summary>
    public class DiscordManager
    {
        private DiscordClient _discord;
        private IConfig _config;

        public DiscordManager(DiscordClient discord, IConfig config)
        {
            _discord = discord;
            _config = config;
        }

        public async Task SendTextMessageToChannelNameOnAllServers(string channelName, string mesage)
        {
            var channels = _discord.Servers.Select(x => x.TextChannels.FirstOrDefault(y => channelName.Equals(y.Name, StringComparison.OrdinalIgnoreCase))).Where(x => x != null);
            foreach (var channel in channels)
            {
                await channel.SendMessage(mesage);
            }
        }

        public async Task EditChannelByNameOnAllServers(string infoTopicChannel, string name = null, string topic = null, int? position = null)
        {
            var channels = _discord.Servers.Select(x => x.TextChannels.FirstOrDefault(y => _config.InfoTopicChannel.Equals(y.Name, StringComparison.OrdinalIgnoreCase))).Where(x => x != null);
            foreach (var channel in channels)
            {
                await channel.Edit(name, topic, position);
            }
        }
    }
}
