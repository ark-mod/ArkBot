using System;
using System.Collections.Generic;

namespace ArkBot.WebApi.Model
{
    public class TribeReferenceViewModel
    {
        public TribeReferenceViewModel()
        {
            MemberSteamIds = new List<string>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> MemberSteamIds { get; set; }
        public DateTime LastActiveTime { get; set; }
    }
}
