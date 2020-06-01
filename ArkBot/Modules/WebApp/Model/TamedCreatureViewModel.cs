﻿using Newtonsoft.Json;
using System;

namespace ArkBot.Modules.WebApp.Model
{
    public class TamedCreatureViewModel
    {
        public TamedCreatureViewModel()
        {
            Aliases = new string[] { };
        }

        public long Id1 { get; set; }
        public long Id2 { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Species { get; set; }
        public string[] Aliases { get; set; }
        public string Gender { get; set; }
        public int BaseLevel { get; set; }
        public int Level { get; set; }
        public float Experience { get; set; }
        public float? BabyAge { get; set; }
        public float? Imprint { get; set; }
        public float? FoodStatus { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float? TopoMapX { get; set; }
        public float? TopoMapY { get; set; }
        public DateTime? NextMating { get; set; }
        public DateTime? BabyFullyGrown { get; set; }
        public DateTime? BabyNextCuddle { get; set; }
        public string OwnerType { get; set; }
        public bool InCryopod { get; set; }
        public int RandomMutationsFemale { get; set; }
        public int RandomMutationsMale { get; set; }


        // these fields are only set when creature is owned by the authenticated person making the request
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CreatureStatsViewModel BaseStats { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CreatureStatsViewModel TamedStats { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CreatureStatValuesViewModel StatValues { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CreatureParentsViewModel Parents { get; set; }
    }
}