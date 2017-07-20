using ArkBot.WebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Helpers
{
    public static class WebApiHelper
    {
        public static UserViewModel GetUser(HttpRequestMessage request)
        {
            var ctx = request.GetOwinContext();
            var authuser = ctx.Authentication.User;
            var name = authuser?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
            var steamId = authuser?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            if (steamId != null) steamId = steamId.Replace("http://steamcommunity.com/openid/id/", "");

            if (authuser != null)
            {
                return new UserViewModel
                {
                    Name = name,
                    SteamId = steamId
                };
            }

            return null;
        }
    }
}
