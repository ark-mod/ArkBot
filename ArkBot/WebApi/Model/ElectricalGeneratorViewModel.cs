﻿namespace ArkBot.WebApi.Model
{
    public class ElectricalGeneratorViewModel : StructureBase
    {
        public ElectricalGeneratorViewModel(ArkSavegameToolkitNet.Domain.ArkLocation location) : base(location)
        {
        }

        //public double? FuelTime { get; set; }
        public int GasolineQuantity { get; set; }
        public bool Activated { get; set; }
    }
}
