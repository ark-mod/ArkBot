using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Database.Model
{
    public class WildCreatureLogEntry
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int Count { get; set; }

        [Column("Ids", TypeName="ntext")]
        [MaxLength]
        private string IdsAsStrings { get; set; }

        private long[] _ids;
        [NotMapped]
        public long[] Ids
        {
            get { return _ids ?? (_ids = IdsAsStrings.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => long.Parse(x, CultureInfo.InvariantCulture)).ToArray()); }
            set
            {
                _ids = null;
                IdsAsStrings = string.Join(",", value.Select(x => x.ToString(CultureInfo.InvariantCulture)));
            }
        }

        public int LogId { get; set; }

        public virtual WildCreatureLog Log { get; set; }
    }
}
