using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class SavedState : ISavedState
    {
        internal string _Path { get; set; }

        public SavedState(string path) : this()
        {
            _Path = path;
        }

        public SavedState()
        {
            PlayerLastActive = new List<PlayerLastActiveSavedState>();
        }

        [JsonProperty(PropertyName = "latestTribeLogDay")]
        public int LatestTribeLogDay { get; set; }

        [JsonProperty(PropertyName = "latestTribeLogTime")]
        public TimeSpan LatestTribeLogTime { get; set; }

        [JsonProperty(PropertyName = "votingDisabled")]
        public bool VotingDisabled { get; set; }

        [JsonProperty(PropertyName = "skipExtractNextRestart")]
        public bool SkipExtractNextRestart { get; set; }
        
        [JsonProperty(PropertyName = "playerLastActive")]
        public List<PlayerLastActiveSavedState> PlayerLastActive { get; set; }

        public bool Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this);
                File.WriteAllText(_Path, json);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
