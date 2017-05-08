using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class TamedCreatureViewModel
    {
        public TamedCreatureViewModel()
        {
            Aliases = new string[] { };
        }

        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Species { get; set; }
        public string[] Aliases { get; set; }
        public string Gender { get; set; }
        public int BaseLevel { get; set; }
        public int Level { get; set; }
        public float? Imprint { get; set; }
        public float? FoodStatus { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float? TopoMapX { get; set; }
        public float? TopoMapY { get; set; }
        public DateTime? NextMating { get; set; }
        public string OwnerType { get; set; }
    }
}
