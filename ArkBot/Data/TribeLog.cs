using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArkBot.Data
{
    public abstract class TribeLog
    {
        public int Day { get; set; }
        public TimeSpan Time { get; set; }
        public string Message { get; set; }
    }

    public class TameWasKilledTribeLog : TribeLog
    {
        public string TribeName { get; set; }
        public int TribeId { get; set; }
        public string Name { get; set; }
        public string SpeciesName { get; set; }
        public int Level { get; set; }
        public string KilledBy { get; set; }
        public int? KilledByLevel { get; set; }

        private static Regex _rParseKilled = new Regex(@"^Day\s+(?<day>\d+),\s+(?<hour>\d{2,2})\:(?<minute>\d{2,2})\:(?<second>\d{2,2})\:\s+.*?Your\s+(?<name>.+?)\s+\-\s+Lvl\s+(?<level>\d+)\s+\((?<species>[^\)]+)\)\s+was\s+killed(?:\!|(?:\s+by\s+a\s+(?<killedBy>.+?)\s+\-\s+Lvl\s+(?<killedByLevel>\d+)\!)).+$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
       
        public static TameWasKilledTribeLog FromLog(string log)
        {
            //relatedlogs
            //"Day 12433, 22:47:56: <RichColor Color=\"1, 0, 0, 1\">Your Direbear AH - Lvl 243 (Dire Bear) was killed by a Dilophosaurus - Lvl 100!</>",
            //"Day 12412, 19:59:51: <RichColor Color=\"1, 0, 0, 1\">Your Giganotosaurus - Lvl 65 (Giganotosaurus) was killed!</>",
            //"Day 12411, 04:31:59: <RichColor Color=\"1, 0, 0, 1\">Your Argentavis B1 - Lvl 178 (Argentavis) was killed by a Megalodon - Lvl 72!</>",
            //"Day 12437, 02:52:26: <RichColor Color=\"1, 0, 0, 1\">Tribemember Tobbe - Lvl 93 was killed by a Raptor - Lvl 56!</>", //we do not parse this

            try
            {
                var m = _rParseKilled.Match(log);
                if (!m.Success) return null; ;

                var item = new TameWasKilledTribeLog
                {
                    Day = int.Parse(m.Groups["day"].Value),
                    Time = new TimeSpan(int.Parse(m.Groups["hour"].Value), int.Parse(m.Groups["minute"].Value), int.Parse(m.Groups["second"].Value)),
                    Name = m.Groups["name"].Value,
                    Level = int.Parse(m.Groups["level"].Value),
                    SpeciesName = m.Groups["species"].Value,
                    KilledBy = m.Groups["killedBy"].Success ? m.Groups["killedBy"].Value : null,
                    KilledByLevel = m.Groups["killedByLevel"].Success ? int.Parse(m.Groups["killedByLevel"].Value) : (int?)null,
                    Message = log
                };
                return item;
            }
            catch { /* ignore exception */}

            return null;
        }
    }
}
