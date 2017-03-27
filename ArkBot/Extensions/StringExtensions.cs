using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArkBot.Extensions
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string self)
        {
            if (self == null) return null;
            if (string.IsNullOrEmpty(self)) return self;

            return self.First().ToString().ToUpper() + self.Substring(1);
        }

        public static string ReplaceRconSpecialChars(this string self)
        {
            if (self == null) return null;
            if (string.IsNullOrEmpty(self)) return self;

            return self.Replace('å', 'a').Replace('ä', 'a').Replace('ö', 'o').Replace('Å', 'A').Replace('Ä', 'A').Replace('Ö', 'O');
        }

        public static string[] Partition(this string self, int partitionMaxLength)
        {
            var result = new List<string>();
            var inword = Environment.NewLine;

            var position = 0;
            while(position + partitionMaxLength < self.Length)
            {
                var index = self.LastIndexOf(inword, position + partitionMaxLength + inword.Length - 2, partitionMaxLength + inword.Length - 1);
                var length = index == -1 || index == position ? partitionMaxLength : index - position + inword.Length > partitionMaxLength ? index - position : index - position + inword.Length;
                result.Add(self.Substring(position, length));
                position += length;
            }

            result.Add(self.Substring(position, self.Length - position));

            return result.ToArray();
        }

        /// <summary>
        /// Computes the levenshtein distance between two input strings
        /// </summary>
        /// <returns>Number of edits required to change one into the other</returns>
        public static int ComputeLevenshteinDistance(this string self, string other)
        {
            if ((self == null && other == null) || self == other) return 0;
            else if (self == null) return other.Length;
            else if (other == null) return self.Length;

            var l = other.Length;
            var arr1 = new int[l + 1];
            var arr2 = new int[l + 1];
            for (var i = 0; i < l + 1; i++) arr1[i] = i;
            for (var i = 0; i < self.Length; i++)
            {
                arr2[0] = i + 1;
                for (var j = 0; j < l; j++)
                {
                    var diff = self[i] == other[j] ? 0 : 1;
                    arr2[j + 1] = Math.Min(Math.Min(arr2[j] + 1, arr1[j + 1] + 1), arr1[j] + diff);
                    arr1[j] = arr2[j];
                }
                arr1[l] = arr2[l];
            }
            return arr2[l];
        }

        /// <summary>
        /// Takes the words from self and computes levenshtein distance for every equal word length combination in findInString and then returns the best match.
        /// </summary>
        /// <returns>No match [null] or [word index, levenshtein distance sum] for the best match</returns>
        public static Tuple<int, int> FindLowestLevenshteinWordDistanceInString(this string self, string findInString)
        {
            if (self == null || findInString == null) return null;

            var words1 = self.GetWords();
            var words2 = findInString.GetWords();
            if (words1 == null || words2 == null || words1.Length < 1 || words2.Length < 1 || words1.Length > words2.Length) return null;

            var levenshteinDistances = new Dictionary<Tuple<string, string>, int>();

            var toMatch = Enumerable.Range(0, words2.Length - words1.Length + 1).Select(i => new { index = i, two = words2.Skip(i).Take(words1.Length).ToArray() });
            var results = toMatch.Select((x, i) => new {
                index = x.index,
                cost = words1.Select((y, j) =>
                {
                    var key = Tuple.Create(y, x.two[j]);

                    int distance;
                    if (!levenshteinDistances.TryGetValue(key, out distance))
                    {
                        distance = y.ComputeLevenshteinDistance(x.two[j]);
                        levenshteinDistances.Add(key, distance);
                    }

                    return distance;
                }).Sum()
            }).ToArray();
            var best = results.OrderBy(x => x.cost).FirstOrDefault();

            return best != null ? new Tuple<int, int>(best.index, best.cost) : null;
        }

        public static string[] GetWords(this string self)
        {
            if (self == null) return null;
            return Regex.Matches(self, @"\w+[^\s]*\w+|\w").OfType<Match>().Select(x => x.Value).ToArray();
        }

        public static int[] IndexOfAll(this string self, char c, int startIndex = 0, StringComparison comp = StringComparison.OrdinalIgnoreCase)
        {
            var indices = new List<int>();
            int index = -1;
            var position = startIndex;

            while (position <= self.Length - 1 && (index = self.IndexOf(new string(c, 1), position, comp)) != -1)
            {
                indices.Add(index);

                position = index + 1;
            }

            return indices.ToArray();
        }

        public static int[] IndexOfAll(this string self, string substring, int startIndex = 0, StringComparison comp = StringComparison.OrdinalIgnoreCase)
        {
            var indices = new List<int>();
            int index = -1;
            var position = startIndex;

            while (position <= self.Length - 1 && (index = self.IndexOf(substring, position, comp)) != -1)
            {
                indices.Add(index);

                position = index + substring.Length;
            }

            return indices.ToArray();
        }

        public static string Join(this string[] self, Func<int, int, string> separatorSelector)
        {
            if (self == null || separatorSelector == null) return null;
            if (self.Length == 0) return string.Empty;
            if (self.Length == 1) return self[0];

            var sb = new StringBuilder();
            var n = 0;
            foreach(var str in self)
            {
                if (n > 0) sb.Append(separatorSelector(n, self.Length - 1));
                sb.Append(str);
                n++;
            }

            return sb.ToString();
        }

        public static string PadBoth(this string self, int totalWidth)
        {
            var padAmount = totalWidth - self.Length;
            if (padAmount <= 1)
            {
                if (padAmount == 1) self.PadRight(totalWidth);
                return self;
            }

            var padLeft = (int)Math.Floor(padAmount / 2d) + self.Length;
            return self.PadLeft(padLeft).PadRight(totalWidth);
        }
    }
}
