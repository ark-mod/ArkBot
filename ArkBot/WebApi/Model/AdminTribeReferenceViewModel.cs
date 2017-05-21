using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class AdminTribeReferenceViewModel : TribeReferenceViewModel
    {
        public int CreatureCount { get; set; }
        public int StructureCount { get; set; }
    }
}
