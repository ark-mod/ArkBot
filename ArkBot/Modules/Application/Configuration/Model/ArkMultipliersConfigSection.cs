using Newtonsoft.Json;
using PropertyChanged;
using System.ComponentModel.DataAnnotations;
using Validar;

namespace ArkBot.Modules.Application.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class ArkMultipliersConfigSection
    {
        public ArkMultipliersConfigSection()
        {
            EggHatchSpeedMultiplier = 1d;
            BabyMatureSpeedMultiplier = 1d;
            CuddleIntervalMultiplier = 1d;
        }

        public override string ToString() => "ARK Multipliers";

        [JsonProperty(PropertyName = "eggHatchSpeedMultiplier")]
        [Display(Name = "Egg Hatch Speed Multiplier", Description = "Pregnancy/incubation time multiplier")]
        public double EggHatchSpeedMultiplier { get; set; }

        [JsonProperty(PropertyName = "babyMatureSpeedMultiplier")]
        [Display(Name = "Baby Mature Speed Multiplier", Description = "Baby mature time multiplier")]
        public double BabyMatureSpeedMultiplier { get; set; }

        [JsonProperty(PropertyName = "cuddleIntervalMultiplier")]
        [Display(Name = "Cuddle Interval Multiplier", Description = "Multiplier for duration between cuddles")]
        public double CuddleIntervalMultiplier { get; set; }
    }
}
