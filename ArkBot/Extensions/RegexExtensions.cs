using System;
using System.Text.RegularExpressions;

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
