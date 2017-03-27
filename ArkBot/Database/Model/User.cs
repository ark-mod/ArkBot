using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Database.Model
{
    public class User
    {
        public User()
        {
            Played = new List<PlayedEntry>();
            Votes = new List<UserVote>();
        }

        public int Id { get; set; }
        public long DiscordId { get; set; }
        public long SteamId { get; set; }
        public string RealName { get; set; }
        public string SteamDisplayName { get; set; }
        public bool DisallowVoting { get; set; }
        public bool Unlinked { get; set; }
        public virtual ICollection<PlayedEntry> Played { get; set; }
        public virtual ICollection<UserVote> Votes { get; set; }
    }
}
