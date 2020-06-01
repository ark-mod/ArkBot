using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArkBot.Modules.Database.Model
{
    public class LoggedLocation
    {
        public LoggedLocation()
        {
        }

        public int Id { get; set; }
        [ForeignKey(nameof(Model.Player))]
        public ulong SteamId { get; set; }
        public DateTime At { get; set; }
        public string ServerKey { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }

        public Player Player { get; set; }
    }
}