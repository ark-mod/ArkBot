namespace ArkBot.Modules.WebApp.Model
{
    public class GeneratorViewModel : StructureBase
    {
        public GeneratorViewModel(ArkSavegameToolkitNet.Domain.ArkLocation location) : base(location)
        {
        }

        //public double? FuelTime { get; set; }
        public int FuelQuantity { get; set; }
        public bool Activated { get; set; }
    }
}
