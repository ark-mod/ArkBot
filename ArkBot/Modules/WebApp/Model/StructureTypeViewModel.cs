using System;

namespace ArkBot.Modules.WebApp.Model
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
