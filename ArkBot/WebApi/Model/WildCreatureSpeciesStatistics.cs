namespace ArkBot.WebApi.Model
{
    public class WildCreatureSpeciesStatistics
    {
        public WildCreatureSpeciesStatistics()
        {
            Aliases = new string[] { };
        }

        public string ClassName { get; set; }
        public string Name { get; set; }
        public string[] Aliases { get; set; }
        public int Count { get; set; }
        public float Fraction { get; set; }
    }
}
