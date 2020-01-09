﻿using System.Collections.Generic;

namespace ArkBot.WebApi.Model
{
    public class WildCreatureSpeciesViewModel
    {
        public WildCreatureSpeciesViewModel()
        {
            Aliases = new string[] { };
            Creatures = new List<WildCreatureViewModel>();
        }

        public string ClassName { get; set; }
        public string Name { get; set; }
        public string[] Aliases { get; set; }
        public List<WildCreatureViewModel> Creatures { get; set; }
        public bool IsTameable { get; set; }
    }
}