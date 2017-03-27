using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Extensions
{
    public static class TimeSpanExtensions
    {
        public static string ToStringCustom(this TimeSpan self, bool showSeconds = false)
        {
            return string.Join(" ", new[] {
                (self.Days > 0 ? self.ToString("d'd'") : null), //player.Time.ToString("hh':'mm")
                (self.Hours > 0 ? self.ToString("h'h'") : null),
                (self.Minutes > 0 ? self.ToString("m'm'") : null),
                (showSeconds && self.Seconds > 0 ? self.ToString("s's'") : null)
                }.Where(x => x != null).ToArray());
        }
    }
}
