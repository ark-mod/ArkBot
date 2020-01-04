using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArkBot.Data
{
    public class TribeLog
    {
        public int Day { get; set; }
        public TimeSpan Time { get; set; }
        public string Message { get; set; }
        public string MessageHtml
        {
            get
            {
                return _rHtmlColors.Replace(Message, (m) =>
                {
                    if (!float.TryParse(m.Groups["r"]?.Value, out float r)) r = 0;
                    if (!float.TryParse(m.Groups["g"]?.Value, out float g)) g = 0;
                    if (!float.TryParse(m.Groups["b"]?.Value, out float b)) b = 0;
                    if (!float.TryParse(m.Groups["a"]?.Value, out float a)) a = 0;

                    var f = (msg: m.Groups["msg"].Value.TrimEnd('!'), r: (int)Math.Round(r * 100), g: (int)Math.Round(g * 100), b: (int)Math.Round(b * 100), a: a);
                    return $@"<span style=""color: rgba({f.r}%,{f.g}%,{f.b}%,{f.a})"">{f.msg}</span>";
                });
            }
        }
        public string MessageUnformatted {  get { return _rRemoveColors.Replace(Message, "").TrimEnd('!'); } }

        public string Raw { get; set; }

        private static Regex _rParseLog = new Regex(@"^Day\s+(?<day>\d+),\s+(?<hour>\d{2,2})\:(?<minute>\d{2,2})\:(?<second>\d{2,2})\:\s+(?<message>.+)$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static Regex _rHtmlColors = new Regex(@"\<RichColor Color\=\""(?<r>[\d\.]+),\s*(?<g>[\d\.]+),\s*(?<b>[\d\.]+),\s*(?<a>[\d\.]+)\""\>(?<msg>.+?)\<\/\>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static Regex _rRemoveColors = new Regex(@"((\<RichColor Color\=\""[\d\.]+,\s*[\d\.]+,\s*[\d\.]+,\s*[\d\.]+\""\>)|(\<\/\>))", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public string ToStringPretty()
        {
            return $"Day {Day}, {Time:hh':'mm':'ss}: {MessageUnformatted}";
        }

        public static TribeLog FromLog(string log)
        {
            try
            {
                var m = _rParseLog.Match(log);
                if (!m.Success) return null;

                var item = new TribeLog
                {
                    Day = int.Parse(m.Groups["day"].Value),
                    Time = new TimeSpan(int.Parse(m.Groups["hour"].Value), int.Parse(m.Groups["minute"].Value), int.Parse(m.Groups["second"].Value)),
                    Message = m.Groups["message"].Value,
                    Raw = log
                };
                return item;
            }
            catch { /* ignore exception */}

            return null;
        }
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
       
        public static new TameWasKilledTribeLog FromLog(string log)
        {
            //relatedlogs
            //"Day 12433, 22:47:56: <RichColor Color=\"1, 0, 0, 1\">Your Direbear AH - Lvl 243 (Dire Bear) was killed by a Dilophosaurus - Lvl 100!</>",
            //"Day 12412, 19:59:51: <RichColor Color=\"1, 0, 0, 1\">Your Giganotosaurus - Lvl 65 (Giganotosaurus) was killed!</>",
            //"Day 12411, 04:31:59: <RichColor Color=\"1, 0, 0, 1\">Your Argentavis B1 - Lvl 178 (Argentavis) was killed by a Megalodon - Lvl 72!</>",
            //"Day 12437, 02:52:26: <RichColor Color=\"1, 0, 0, 1\">Tribemember Tobbe - Lvl 93 was killed by a Raptor - Lvl 56!</>", //we do not parse this
            //"Day 12437, 02:52:26: <RichColor Color=\"0.85, 0.15, 0.25, 1\">HaYoon uploaded a Daeodon: Boss 84k - Lvl 256</>"

            try
            {
                var m = _rParseKilled.Match(log);
                if (!m.Success) return null;

                var item = new TameWasKilledTribeLog
                {
                    Day = int.Parse(m.Groups["day"].Value),
                    Time = new TimeSpan(int.Parse(m.Groups["hour"].Value), int.Parse(m.Groups["minute"].Value), int.Parse(m.Groups["second"].Value)),
                    Name = m.Groups["name"].Value,
                    Level = int.Parse(m.Groups["level"].Value),
                    SpeciesName = m.Groups["species"].Value,
                    KilledBy = m.Groups["killedBy"].Success ? m.Groups["killedBy"].Value : null,
                    KilledByLevel = m.Groups["killedByLevel"].Success ? int.Parse(m.Groups["killedByLevel"].Value) : (int?)null,
                    Message = null,
                    Raw = log
                };
                return item;
            }
            catch { /* ignore exception */}

            return null;
        }
    }
}
