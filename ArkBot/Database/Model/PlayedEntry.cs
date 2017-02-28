using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Database.Model
{
    public class PlayedEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public long TimeInSeconds { get; set; }
        public int? UserId { get; set; }
        public virtual User User { get; set; }
        public long? SteamId { get; set; }
    }
}
