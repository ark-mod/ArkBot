using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkBot.Configuration;
using ArkBot.Configuration.Validation;
using Discord;
using Microsoft.IdentityModel;
using PropertyChanged;
using Validar;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace ArkBot.Configuration.Model
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
