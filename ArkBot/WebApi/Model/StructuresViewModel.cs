using System.Collections.Generic;

namespace ArkBot.WebApi.Model
{
    public class StructuresViewModel
    {
        public StructuresViewModel()
        {
            Areas = new List<StructureAreaViewModel>();
            Types = new List<StructureTypeViewModel>();
            Owners = new List<StructureOwnerViewModel>();
        }
        public string MapName { get; set; }
        public List<StructureAreaViewModel> Areas { get; set; }
        public List<StructureTypeViewModel> Types { get; set; }
        public List<StructureOwnerViewModel> Owners { get; set; }
    }
}
