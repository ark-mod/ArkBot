using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Database.Model
{
    public class TamedCreatureLogEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        public DateTime LastSeen { get; set; }
        public string RelatedLogEntries { get; set; }

        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal Z { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int? Team { get; set; }
        public int? PlayerId { get; set; }
        public bool Female { get; set; }
        public decimal? TamedAtTime { get; set; }
        public decimal? TamedTime { get; set; }
        public string Tribe { get; set; }
        public string Tamer { get; set; }
        public string OwnerName { get; set; }
        public string Name { get; set; }
        public int BaseLevel { get; set; }
        public int? FullLevel { get; set; }
        public decimal? Experience { get; set; }
        public double? ApproxFoodPercentage { get; set; }
        public double? ApproxHealthPercentage { get; set; }
        public decimal? ImprintingQuality { get; set; }
        public string SpeciesClass { get; set; }
        public bool IsConfirmedDead { get; set; }
        public bool IsInCluster { get; set; }

        /// <summary>
        /// Could be due to confirmed death, unconfirmed death, cluster upload or possibly a bug
        /// </summary>
        public bool IsUnavailable { get; set; }
    }
}
