using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class CreatureBaseStatsViewModel
    {
        public int Health { get; set; }
        public int Stamina { get; set; }
        public int Oxygen { get; set; }
        public int Food { get; set; }
        public int Weight { get; set; }
        public int Melee { get; set; }
        public int MovementSpeed { get; set; }
    }
}
