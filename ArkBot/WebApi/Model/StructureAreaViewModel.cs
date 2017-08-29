using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class StructureAreaViewModel
    {
        public StructureAreaViewModel()
        {
            Structures = new List<StructureViewModel>();
        }

        public int OwnerId { get; set; }
        public List<StructureViewModel> Structures { get; set; }
        public int StructureCount { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float TopoMapX { get; set; }
        public float TopoMapY { get; set; }
        public float Radius { get; set; }
        public float RadiusPx { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float RadiusUu { get; set; }
        public float TrashQuota { get; set; }
    }
}
