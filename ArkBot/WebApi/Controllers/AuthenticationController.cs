using ArkBot.Configuration.Model;
using ArkBot.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Controllers
{
    public class AuthenticationController : BaseApiController
    {
        public AuthenticationController(IConfig config) : base(config)
        {
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromForm] string returnUrl = null)
        {
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl ?? "/" }, "Steam");

            //var properties = new AuthenticationProperties() { RedirectUri = Url.Link("DefaultAuth", new { Controller = "Authentication", Action = "LoginCallback", returnUrl = returnUrl }) };
            //await HttpContext.ChallengeAsync("Steam", properties);

            //return Unauthorized(); //{ RequestMessage = Request }
        }

        [HttpGet("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(string returnUrl)
        {
            var ctx = HttpContext;
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Redirect(returnUrl);
        }

        [HttpGet("logincallback")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginCallback(string returnUrl)
        {
            var ctx = HttpContext;
            var result = await ctx.AuthenticateAsync("ExternalCookie");
            if (result == null) return BadRequest(); //{ RequestMessage = Request }

            await ctx.SignOutAsync("ExternalCookie");

            var claims = result?.Principal.Claims.ToList();
            claims.Add(new Claim(ClaimTypes.AuthenticationMethod, "Steam"));

            var steamId = claims?.FirstOrDefault(x => x.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
            if (steamId != null)
            {
                steamId = steamId.Replace("http://steamcommunity.com/openid/id/", "");
                steamId = steamId.Replace("https://steamcommunity.com/openid/id/", "");
            }
            if (!string.IsNullOrEmpty(steamId))
            {
                var roles = WebApiHelper.GetRolesForUser(_config, steamId);

                foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var ci = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await ctx.SignInAsync(new ClaimsPrincipal(ci));

            return Redirect(returnUrl);
        }
    }
}
