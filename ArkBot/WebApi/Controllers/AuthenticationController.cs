using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace ArkBot.WebApi.Controllers
{
    public class AuthenticationController : ApiController
    {
        [HttpPost]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> Login(FormDataCollection formData)
        {
            var returnUrl = formData?["returnUrl"];
            var properties = new AuthenticationProperties() { RedirectUri = Url.Link("DefaultAuth", new { Controller = "Authentication", Action = "LoginCallback", returnUrl = returnUrl }) };
            Request.GetOwinContext().Authentication.Challenge(properties, "Steam");

            return new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = Request };
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Logout(string returnUrl)
        {
            var ctx = Request.GetOwinContext();
            ctx.Authentication.SignOut("Cookie");

            var response = Request.CreateResponse(HttpStatusCode.Redirect);
            response.Headers.Location = new Uri(returnUrl);
            return response;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> LoginCallback(string returnUrl)
        {
            var ctx = Request.GetOwinContext();
            var result = await ctx.Authentication.AuthenticateAsync("ExternalCookie");
            if (result == null) return new HttpResponseMessage(HttpStatusCode.BadRequest) { RequestMessage = Request };

            ctx.Authentication.SignOut("ExternalCookie");

            var claims = result?.Identity.Claims.ToList();
            claims.Add(new Claim(ClaimTypes.AuthenticationMethod, "Steam"));

            var ci = new ClaimsIdentity(claims, "Cookie");
            ctx.Authentication.SignIn(ci);

            var response = Request.CreateResponse(HttpStatusCode.Redirect);
            response.Headers.Location = new Uri(returnUrl);
            return response;
        }
    }
}
