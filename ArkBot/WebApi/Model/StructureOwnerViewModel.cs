using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class StructureOwnerViewModel
    {
        internal Lazy<int> _generateId;

        public StructureOwnerViewModel(Lazy<int> generateId)
        {
            _generateId = generateId;
            Id = -1;
        }

        public StructureOwnerViewModel() { }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int OwnerId { get; set; }
        public DateTime? LastActiveTime { get; set; }
        public int AreaCount { get; set; }
        public int StructureCount { get; set; }
        public int CreatureCount { get; set; }
    }
}
