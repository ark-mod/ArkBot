using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArkBot.Modules.Database.Model
{
    public class Player
    {
        public Player()
        {
            ChatMessages = new List<ChatMessage>();
            LoggedLocations = new List<LoggedLocation>();
        }

        /// <summary>
        /// Steam ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }
        public string LastServerKey { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime LastActive { get; set; }
        public bool IsOnline { get; set; }

        public List<ChatMessage> ChatMessages { get; set; }
        public List<LoggedLocation> LoggedLocations { get; set; }
    }
}
