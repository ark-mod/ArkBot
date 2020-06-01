using ArkBot.Modules.WebApp.Hubs;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArkBot.Modules.Database.Model
{
    public class ChatMessage
    {
        public ChatMessage()
        {
        }

        public int Id { get; set; }
        public DateTime At { get; set; }
        public string ServerKey { get; set; }

        [ForeignKey(nameof(Model.Player))]
        public ulong SteamId { get; set; }
        public string PlayerName { get; set; }
        public string CharacterName { get; set; }
        public string TribeName { get; set; }
        public string Message { get; set; }
        public ChatMode Mode { get; set; }
        public ChatIcon Icon { get; set; }

        public Player Player { get; set; }
    }
}