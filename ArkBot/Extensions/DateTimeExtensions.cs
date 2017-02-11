using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToStringWithRelativeDay(this DateTime self, string formatFull = "yyyy-MM-dd HH:mm", string formatTime = "HH:mm", string today = "today at", string yesterday = "yesterday at")
        {
            var isToday = DateTime.Today == self.Date;
            var isYesterday = DateTime.Today.AddDays(-1).Date == self.Date;
            var str = isToday ? self.ToString("'" + today + "' " + formatTime) : isYesterday ? self.ToString("'" + yesterday + "' " + formatTime) : self.ToString(formatFull);

            return str;
        }
    }
}
