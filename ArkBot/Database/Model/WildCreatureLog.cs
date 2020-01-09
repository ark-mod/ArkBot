using System;
using System.Collections.Generic;

namespace ArkBot.Database.Model
{
    public class WildCreatureLog
    {
        public WildCreatureLog()
        {
            Entries = new List<WildCreatureLogEntry>();
        }

        public int Id { get; set; }

        public DateTime When { get; set; }

        public virtual ICollection<WildCreatureLogEntry> Entries { get; set; }
    }
}
