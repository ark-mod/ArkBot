using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class StructureTypeViewModel
    {
        internal Lazy<int> _generateId;

        public StructureTypeViewModel(Lazy<int> generateId)
        {
            _generateId = generateId;
            Id = -1;
        }

        public StructureTypeViewModel() { }

        public int Id { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }
    }
}
