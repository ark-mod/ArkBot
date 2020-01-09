using Newtonsoft.Json;

namespace ArkBot.Data
{
    public class Cluster
    {
        public Cluster()
        {
            Creatures = new Creature[] { };
        }

        [JsonProperty(PropertyName = "creatures")]
        public Creature[] Creatures { get; set; }
    }
}
