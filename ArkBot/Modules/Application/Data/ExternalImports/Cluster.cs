using Newtonsoft.Json;

namespace ArkBot.Modules.Application.Data.ExternalImports
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
