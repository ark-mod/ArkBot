namespace ArkBot.WebApi.Model
{
    public class CloudCreatureViewModel
    {
        public CloudCreatureViewModel()
        {
            Aliases = new string[] { };
        }

        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Species { get; set; }
        public string[] Aliases { get; set; }
        public int? Level { get; set; }
    }
}
