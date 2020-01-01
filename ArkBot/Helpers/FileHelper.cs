using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Helpers
{
    public static class FileHelper
    {
        public static bool IsValidDirectoryPath(string path, bool allowRelativePath = true)
        {
            if (path == null) return false;

            try
            {
                var invalidChars = Path.GetInvalidPathChars();
                if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1) return false;
                if (!allowRelativePath && !Path.IsPathRooted(path)) return false;

                var p = Path.GetFullPath(path);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static void CreateZipArchive(string[] files, string path, string basePath = null)
        {
            using (var fileStream = new FileStream(path, FileMode.CreateNew))
            {
                using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        var entry = archive.CreateEntryFromFile(
                            file,
                            basePath == null || !file.StartsWith(basePath, StringComparison.OrdinalIgnoreCase)
                                ? Path.GetFileName(file) :
                                file.Substring(basePath.Length).TrimStart('\\'), CompressionLevel.Fastest);
                    }
                }
            }
        }

        public static Dictionary<string, string> ExtractFilesInZipFile(string path, string[] pathsInArchive)
        {
            if (!File.Exists(path)) return null;

            var result = new Dictionary<string, string>();

            try
            {
                using (var zip = Ionic.Zip.ZipFile.Read(path))
                {
                    foreach (var entry in zip.Where(x => pathsInArchive
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Contains(x.FileName, StringComparer.OrdinalIgnoreCase)))
                    {
                        var outFilePath = Path.GetTempFileName();
                        using (FileStream fs = new FileStream(outFilePath, FileMode.Truncate, FileAccess.Write))
                        {
                            entry.Extract(fs);
                        }

                        result.Add(entry.FileName, outFilePath);
                    }
                }
            }
            catch { }

            return result;
        }

        public static string ExtractFileInZipFile(string path, string pathInArchive)
        {
            return ExtractFilesInZipFile(path, new[] { pathInArchive })?.Values.FirstOrDefault();
        }

        public static string[] GetZipFileContents(string path)
        {
            if (!File.Exists(path)) return null;

            try
            {
                using (var zip = Ionic.Zip.ZipFile.Read(path))
                {
                    return zip.EntryFileNames.ToArray();
                }
            }
            catch { }

            return null;
        }

        public static string[] CreateDotNetZipArchive(Tuple<string, string, string[]>[] files, string path, int? maxSegmentSizeBytes = null, Ionic.Zlib.CompressionLevel compressionLevel = Ionic.Zlib.CompressionLevel.BestCompression)
        {
            using (var zip = new Ionic.Zip.ZipFile { CompressionLevel = compressionLevel })
            {
                var entities = files.SelectMany(x =>
                    x.Item3.Select(y => new
                    {
                        path = y,
                        directoryPathInArchive = string.IsNullOrWhiteSpace(x.Item1) || !y.StartsWith(x.Item1, StringComparison.OrdinalIgnoreCase)
                            ? x.Item2 ?? ""
                            : Path.Combine(x.Item2 ?? "", Path.GetDirectoryName(y.Substring(x.Item1.Length).TrimStart('\\')))
                    })).ToArray();
                foreach (var entity in entities) zip.AddFile(entity.path, entity.directoryPathInArchive);
                if(maxSegmentSizeBytes.HasValue) zip.MaxOutputSegmentSize = maxSegmentSizeBytes.Value;
                zip.Save(path);

                return Enumerable.Range(0, maxSegmentSizeBytes.HasValue ? zip.NumberOfSegmentsForMostRecentSave : 1)
                    .Select(x => x == 0 ? path : Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".z" + (x < 10 ? "0" : "") + x)).ToArray();
            }
        }

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
