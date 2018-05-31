using ArkBot.Configuration.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Services
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private Google.Apis.Urlshortener.v1.UrlshortenerService _urlShortenerService;
        private IConfig _config;

        public UrlShortenerService(IConfig config)
        {
            _config = config;
            _urlShortenerService = new Google.Apis.Urlshortener.v1.UrlshortenerService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                ApiKey = _config.GoogleApiKey,
                ApplicationName = _config.BotName,
            });
        }

        public async Task<string> ShortenUrl(string longUrl)
        {
            var url = new Google.Apis.Urlshortener.v1.Data.Url
            {
                LongUrl = longUrl
            };
            return (await _urlShortenerService.Url.Insert(url).ExecuteAsync())?.Id;
        }
    }
}
