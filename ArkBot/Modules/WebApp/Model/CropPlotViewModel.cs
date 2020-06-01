﻿using System.Collections.Generic;

namespace ArkBot.Modules.WebApp.Model
{
    public class CropPlotViewModel : StructureBase
    {
        static readonly private Dictionary<string, string> _sizes = new Dictionary<string, string>
        {
            { "CropPlotSmall_SM_C", "Small" },
            { "CropPlotMedium_SM_C", "Medium" },
            { "CropPlotLarge_SM_C", "Large" },
            { "BP_CropPlot_Small_C", "Small" },
            { "BP_CropPlot_Medium_C", "Medium" },
            { "BP_CropPlot_Large_C", "Large" }
        };

        public CropPlotViewModel(ArkSavegameToolkitNet.Domain.ArkLocation location) : base(location)
        {
        }

        public string ClassName { get; set; }
        //public float FertilizerAmount { get; set; }
        public int FertilizerQuantity { get; set; }
        public int FertilizerMax => 54000 * 30;
        public float WaterAmount { get; set; }
        public string PlantedCropClassName { get; set; }
        public string PlantedCropName { get; set; }

        public string Size
        {
            get
            {
                string size = null;
                if (ClassName == null || !_sizes.TryGetValue(ClassName, out size)) return null;

                return size;
            }
        }
    }
}
