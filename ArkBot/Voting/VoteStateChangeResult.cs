﻿using System;
using System.Threading.Tasks;

namespace ArkBot.Voting
{
    public class VoteStateChangeResult
    {
        public string MessageRcon { get; set; }
        public string MessageAnnouncement { get; set; }
        public Func<Task> React { get; set; }
        public int ReactDelayInMinutes { get; set; }
        public string ReactDelayFor { get; set; }
    }
}
