using ArkBot.Configuration.Model;
using Nancy.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Services
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private IConfig _config;

        public UrlShortenerService(IConfig config)
        {
            _config = config;
        }

        public async Task<string> ShortenUrl(string longUrl)
        {
            var url = string.Format("https://api-ssl.bitly.com/v3/shorten?access_token={0}&longUrl={1}", _config.BitlyApiKey, HttpUtility.UrlEncode(longUrl));
            var request = (HttpWebRequest)WebRequest.Create(url);

            try
            {
                var response = await request.GetResponseAsync();
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    var jsonResponse = JObject.Parse(await reader.ReadToEndAsync());
                    var statusCode = jsonResponse["status_code"].Value<int>();
                    if (statusCode == (int)HttpStatusCode.OK)
                        return jsonResponse["data"]["url"].Value<string>();

                    Logging.Log(String.Join("Bitly request returned error code {0}, status text '{1}' on longUrl = {2}", statusCode, jsonResponse["status_txt"].Value<string>(), longUrl), GetType());
                    return longUrl;
                }
            }
            catch (WebException ex)
            {
                Logging.LogException("Bitly Url Service", ex, GetType());
                return longUrl;
            }
        }
    }
}
