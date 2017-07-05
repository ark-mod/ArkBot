using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class CropPlotViewModel : StructureBase
    {
        static readonly private Dictionary<string, string> _crops = new Dictionary<string, string>
        {
            { "PrimalItemConsumable_Seed_DefensePlant_C", "Plant Species X" },
            { "PrimalItemConsumable_Seed_Rockarrot_C", "Rockarrot" },
            { "PrimalItemConsumable_Seed_Longrass_C", "Longrass" },
            { "PrimalItemConsumable_Seed_Savoroot_C", "Savoroot" },
            { "PrimalItemConsumable_Seed_Citronal_C", "Citronal" },
            { "PrimalItemConsumable_Berry_Amarberry_C", "Amarberry" },
            { "PrimalItemConsumable_Berry_Azulberry_C", "Azulberry" },
            { "PrimalItemConsumable_Berry_Tintoberry_C", "Tintoberry" },
            { "PrimalItemConsumable_Berry_Stimberry_C", "Stimberry" },
            { "PrimalItemConsumable_Berry_Narcoberry_C", "Narcoberry" }
        };

        static readonly private Dictionary<string, string> _sizes = new Dictionary<string, string>
        {
            { "CropPlotSmall_SM_C", "Small" },
            { "CropPlotMedium_SM_C", "Medium" },
            { "CropPlotLarge_SM_C", "Large" }
        };

        public CropPlotViewModel(ArkSavegameToolkitNet.Domain.ArkLocation location) : base(location)
        {
        }

        public string ClassName { get; set; }
        //public float FertilizerAmount { get; set; }
        public int FertilizerQuantity { get; set; }
        public int FertilizerMax => 54000 * 10;
        public float WaterAmount { get; set; }
        public string PlantedCropClassName { get; set; }
        public string PlantedCropName
        {
            get
            {
                string cropName = null;
                if (PlantedCropClassName == null || !_crops.TryGetValue(PlantedCropClassName, out cropName)) return null;

                return cropName;
            }
        }
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
