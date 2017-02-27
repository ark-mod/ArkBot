using ArkBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Helpers
{
    public static class StatisticsHelper
    {
        /// <summary>
        /// Computes a trimmed collection based on comparing elements using standard deviation
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="comparer">Decides which elements to keep based on [distance from average, standard deviation]</param>
        public static TValue[] FilterUsingStandardDeviation<TValue>(TValue[] array, Func<TValue, double> valueSelector, Func<double, double, bool> comparer, bool sample = true)
        {
            if (valueSelector == null || comparer == null || array == null || array.Length == 0 || (array.Length == 1 && sample == true)) return null;

            var avg = array.Average(x => valueSelector(x));
            var sums = array.Sum(val => (valueSelector(val) - avg) * (valueSelector(val) - avg));
            var vari = sums / (array.Length - (sample ? 1 : 0));
            var sd = Math.Sqrt(vari);

            return array.Where(x => comparer(valueSelector(x) - avg, sd)).ToArray();
        }

        public static double CompareToCharacterSequence(string str, char[] sequence)
        {
            var results = CompareToCharacterSequence(str, sequence, 0, 0, 0d);

            return results.Max();
        }

        private static double[] CompareToCharacterSequence(string str, char[] sequence, int n, int position, double similarity)
        {
            var len = str.Length;
            const double p = 0.3;
            int[] indices;

            if (position >= len || (indices = str.IndexOfAll(sequence[n], position)).Length == 0)
            {
                similarity -= 1d / len;
                return n + 1 >= sequence.Length ? new[] { similarity } : CompareToCharacterSequence(str, sequence, n + 1, position, similarity);
            }

            return indices.SelectMany(index =>
            {
                var isimilarity = similarity + (1d / len) - (Math.Abs(index - position) / len * (1 - p)); // (p + ((1 - p) - ((index - position) / len));
                var iposition = index + 1;

                return n + 1 >= sequence.Length ? new[] { isimilarity } : CompareToCharacterSequence(str, sequence, n + 1, iposition, isimilarity);
            }).ToArray();
        }
    }
}
