using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Helpers
{
    public static class FileHelper
    {
        public static async Task<string> ReadAllTextTaskAsync(string filepath)
        {
            if (!File.Exists(filepath)) return null;

            using(var sr = new StreamReader(filepath))
            {
                return await sr.ReadToEndAsync();
            }
        }

        public static string GetAvailableFilePathSequential(string filePath)
        {
            if (!File.Exists(filePath)) return filePath;

            var ext = Path.GetExtension(filePath);
            var filename = string.IsNullOrEmpty(ext) ? filePath : filePath.Substring(0, filePath.Length - ext.Length);
            string path = null;
            var n = 0;
            while (true)
            {
                path = string.Format(@"{0}{1}{2}", filename, (n > 0 ? "-" + n.ToString("000") : ""), ext);
                if (!File.Exists(path)) return path;
                else n++;
            }
        }

        public static string ToFileSize(this long value)
        {
            return ToFileSize((double)value);
        }

        // Return a string describing the value as a file size.
        // For example, 1.23 MB.
        public static string ToFileSize(double value)
        {
            string[] suffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};
            for (int i = 0; i < suffixes.Length; i++)
            {
                if (value <= (Math.Pow(1024, i + 1)))
                {
                    return ThreeNonZeroDigits(value /
                        Math.Pow(1024, i)) +
                        " " + suffixes[i];
                }
            }

            return ThreeNonZeroDigits(value /
                Math.Pow(1024, suffixes.Length - 1)) +
                " " + suffixes[suffixes.Length - 1];
        }

        // Return the value formatted to include at most three
        // non-zero digits and at most two digits after the
        // decimal point. Examples:
        //1
        //123
        //12.3
        //1.23
        //0.12
        private static string ThreeNonZeroDigits(double value)
        {
            if (value >= 100)
            {
                // No digits after the decimal.
                return value.ToString("0,0");
            }
            else if (value >= 10)
            {
                // One digit after the decimal.
                return value.ToString("0.0");
            }
            else
            {
                // Two digits after the decimal.
                return value.ToString("0.00");
            }
        }
    }
}
