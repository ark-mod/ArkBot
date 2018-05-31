using ArkBot.Configuration.Model;
using ArkBot.WebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Helpers
{
    public static class WebApiHelper
    {
        public static UserViewModel GetUser(HttpRequestMessage request, IConfig config)
        {
            var ctx = request.GetOwinContext();
            var authuser = ctx.Authentication.User;

            return GetUser(authuser, config);
        }

        public static UserViewModel GetUser(ClaimsPrincipal authuser, IConfig config)
        {
            var name = authuser?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
            var steamId = authuser?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            if (steamId != null)
            {
                steamId = steamId.Replace("http://steamcommunity.com/openid/id/", "");
                steamId = steamId.Replace("https://steamcommunity.com/openid/id/", "");
            }

            if (authuser != null)
            {
                var roles = GetRolesForUser(config, steamId);

                return new UserViewModel
                {
                    Name = name,
                    SteamId = steamId,
                    Roles = roles
                };
            }
            else
            {
                return new UserViewModel
                {
                    Roles = new[] { "guest" }
                };
            }
        }

        public static string[] GetRolesForUser(IConfig config, string steamId)
        {
            var roles = (!string.IsNullOrEmpty(steamId) ? config.UserRoles?.Where(x => x.SteamIds?.Contains(steamId) == true).Select(x => x.Role).ToList() : null) ?? new List<string>();

            //default roles
            roles.Add("guest");
            roles.Add("user");
            

            return roles.Distinct().OrderBy(x => x).ToArray();
        }
    }
}
