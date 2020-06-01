﻿using ArkBot.Utils.Extensions;
using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArkBot.Modules.Discord
{
    public class DiscordMessage
    {
        private IMessageChannel _channel;
        private ulong _userId;
        private List<string> _history;
        private IUserMessage _message;
        private Task<IUserMessage> _task;

        public DiscordMessage(IMessageChannel channel, ulong userId)
        {
            _channel = channel;
            _userId = userId;
            _history = new List<string>();
        }

        public async Task<IUserMessage> SendOrUpdateMessageDirectedAt(string text)
        {
            _history.Add(text);
            if (_history.Count <= 1)
            {
                _task = _channel.SendMessageDirectedAt(_userId, text);
                _message = await _task;
                return _message;
            }
            else
            {
                var msg = _message;
                if (msg == null) msg = await _task;

                await msg.ModifyAsync(m => m.Content = text);
                return msg;
            }
        }
    }
}
