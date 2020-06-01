namespace ArkBot.Modules.WebApp.Model
{
    public class CreatureStatValuesViewModel
    {
        public CreatureStatValuesViewModel()
        {
            Tamed = new double[12];
            TamedNoImprint = new double[12];
            Wild = new double[12];
        }

        public double[] Tamed { get; set; }
        public double[] TamedNoImprint { get; set; }
        public double[] Wild { get; set; }
    }
}
