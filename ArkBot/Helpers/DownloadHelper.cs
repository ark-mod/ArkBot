using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Helpers
{
    public static class DownloadHelper
    {
        public static async Task<bool> DownloadLatestReleaseFromGithub(string url, Func<string, bool> assetSelector, string path, Func<string, string> extractFromArchive = null, bool redownloadIfAlreadyExists = false, TimeSpan? redownloadIfOlderThan = null)
        {
            try
            {
                if (File.Exists(path))
                {
                    if (!redownloadIfAlreadyExists) return true;
                    if (redownloadIfOlderThan.HasValue && (DateTime.Now - File.GetCreationTime(path)) <= redownloadIfOlderThan.Value) return true;

                    File.Delete(path);
                }

                using (var wc = new System.Net.WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent, "arkbot");
                    var data = await wc.DownloadStringTaskAsync(url);
                    var json = JsonConvert.DeserializeAnonymousType(data, new { tag_name = "", assets = new[] { new { name = "", browser_download_url = "" } } });
                    var downloadUrl = json?.assets?.FirstOrDefault(x => assetSelector(x.browser_download_url))?.browser_download_url;
                    if (downloadUrl == null || !await DownloadFile(downloadUrl, path, redownloadIfAlreadyExists, redownloadIfOlderThan)) return false;

                    if (extractFromArchive != null)
                    {
                        using (var archive = ZipFile.OpenRead(path))
                        {
                            foreach (var entry in archive.Entries)
                            {
                                string zpath = null;
                                if ((zpath = extractFromArchive(entry.FullName)) == null) continue;

                                entry.ExtractToFile(zpath, true);
                            }
                        }
                    }
                }
            }
            catch (System.Net.WebException ex)
            {
                File.Delete(path);
                return false;
            }
            catch (OperationCanceledException)
            {
                File.Delete(path);
                return false;
            }

            return true;
        }

        public static async Task<bool> DownloadFile(string url, string path, bool redownloadIfAlreadyExists = false, TimeSpan? redownloadIfOlderThan = null)
        {
            try
            {
                if (File.Exists(path))
                {
                    if (!redownloadIfAlreadyExists) return true;
                    if (redownloadIfOlderThan.HasValue && (DateTime.Now - File.GetCreationTime(path)) <= redownloadIfOlderThan.Value) return true;

                    File.Delete(path);
                }
                var dirPath = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dirPath)) Directory.CreateDirectory(dirPath);

                using (var wc = new System.Net.WebClient())
                {
                    await wc.DownloadFileTaskAsync(url, path);
                }
            }
            catch (System.Net.WebException ex)
            {
                if (File.Exists(path)) File.Delete(path);
                return false;
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(path)) File.Delete(path);
                return false;
            }

            return true;
        }
    }
}
