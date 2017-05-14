using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
