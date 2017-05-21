using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class AdminPlayerReferenceViewModel : PlayerReferenceViewModel
    {
        public int StructureCount { get; set; }
        public int CreatureCount { get; set; }
    }
}
