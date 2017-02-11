using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArkBot.Helpers
{
    public static class TimeSpanHelper
    {
        private static Dictionary<string, int> _specifiers;

        static TimeSpanHelper()
        {
            //initialize custom time formatting specifiers collection
            _specifiers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            new[] {
                new { s = new[] { "d", "day", "days" }, ms = 24 * 60 * 60 * 1000 },
                new { s = new[] { "h", "hour", "hours" }, ms = 60 * 60 * 1000 },
                new { s = new[] { "m", "min", "mins", "minutes" }, ms = 60 * 1000 },
                new { s = new[] { "s", "sec", "secs", "seconds" }, ms = 1000 }
            }.ToList().ForEach(x => x.s.ToList().ForEach(y => _specifiers.Add(y, x.ms)));
        }

        /// <summary>
        /// Parse timespan from string formatted as (ex. 1 days 5h 15m 30 sec)
        /// </summary>
        public static TimeSpan? ParseFromString(string timeSpan)
        {
            if (timeSpan == null) return null;

            var r = new Regex(@"^\s*((?<value>\d+)\s*(?<suffix>" + string.Join("|", _specifiers.Keys.Select(x => Regex.Escape(x))) + @")(?:\s+|$))+$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);

            var m = r.Match(timeSpan);
            if (!m.Success) return null;

            var times = m.Groups["value"].Captures.Cast<Capture>().Select((x, i) =>
            {
                int ms = 0;
                int value = 0;
                if (!_specifiers.TryGetValue(m.Groups["suffix"].Captures[i].Value, out ms)
                    || !int.TryParse(x.Value, NumberStyles.None, CultureInfo.InvariantCulture, out value)) return (int?)null;

                return value * ms;
            }).ToArray();

            return times.All(x => x.HasValue) ?
                TimeSpan.FromMilliseconds((double)times.Sum()) : (TimeSpan?)null;
        }
    }
}
