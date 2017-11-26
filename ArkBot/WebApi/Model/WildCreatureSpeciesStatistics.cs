using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
