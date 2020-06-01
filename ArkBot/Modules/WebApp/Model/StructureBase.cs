namespace ArkBot.Modules.WebApp.Model
{
    public abstract class StructureBase
    {
        public StructureBase(ArkSavegameToolkitNet.Domain.ArkLocation location)
        {
            Latitude = location?.Latitude;
            Longitude = location?.Longitude;
            TopoMapX = location?.TopoMapX;
            TopoMapY = location?.TopoMapY;
        }

        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float? TopoMapX { get; set; }
        public float? TopoMapY { get; set; }
    }
}
