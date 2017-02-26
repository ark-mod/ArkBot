using System.Threading.Tasks;

namespace ArkBot.Services
{
    public interface IUrlShortenerService
    {
        Task<string> ShortenUrl(string longUrl);
    }
}