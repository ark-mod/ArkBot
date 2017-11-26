using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Extensions
{
    public static class NumberExtensions
    {
        /// <summary>
        /// Clamps a value to be within a range
        /// </summary>
        /// <returns></returns>
        public static TValue Clamp<TValue>(this TValue v, TValue? min = null, TValue? max = null)
            where TValue: struct, IComparable<TValue>
        {
            return (min.HasValue && v.CompareTo(min.Value) < 0 ? min.Value : max.HasValue && v.CompareTo(max.Value) > 0 ? max.Value : v);
        }

        /// <summary>
        /// Rounds a floating point number
        /// </summary>
        /// <returns></returns>
        public static double Round(this double v, int decimals)
        {
            return Math.Round(v, decimals);
        }

        /// <summary>
        /// Rounds a floating point number
        /// </summary>
        /// <returns></returns>
        public static float Round(this float v, int decimals)
        {
            return (float)Math.Round(v, decimals);
        }
    }
}
