using ArkBot.Database.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class VoteInitiatedEventArgs : EventArgs
    {
        public Database.Model.Vote Item { get; set; }
    }
}
