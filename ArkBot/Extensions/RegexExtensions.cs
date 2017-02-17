using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArkBot.Extensions
{
    public static class RegexExtensions
    {
        public static T Match<T>(this Regex rexpression, string input, Func<Match, T> outputFunc)
        {
            if (input == null) return default(T);

            var m = rexpression.Match(input);
            return outputFunc(m);
        }
    }
}
