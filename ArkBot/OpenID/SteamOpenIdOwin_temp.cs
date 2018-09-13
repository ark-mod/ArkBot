using Microsoft.Owin.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Owin.Security.Providers.OpenIDBase;
using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using System;

// this code is taken from https://github.com/TerribleDev/OwinOAuthProviders/blob/master/src/Owin.Security.Providers.Steam
// it has been changed to work with changes to the steam open id provider

namespace ArkBot.OpenID
{
    /// <summary>
    /// Extension methods for using <see cref="SteamAuthenticationMiddleware"/>
    /// </summary>
    public static class SteamAuthenticationExtensionsNew
    {
        /// <summary>
        /// Authenticate users using Steam
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="options">Middleware configuration options</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseSteamAuthenticationNew(this IAppBuilder app, SteamAuthenticationOptionsNew options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.Use(typeof(SteamAuthenticationMiddlewareNew), app, options);
        }

        /// <summary>
        /// Authenticate users using Steam
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="applicationKey">The steam application key</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseSteamAuthenticationNew(this IAppBuilder app, string applicationKey)
        {
            return UseSteamAuthenticationNew(app, new SteamAuthenticationOptionsNew
            {
                ApplicationKey = applicationKey
            });
        }
    }

    public sealed class SteamAuthenticationOptionsNew : OpenIDAuthenticationOptions
    {
        public string ApplicationKey { get; set; }

        public SteamAuthenticationOptionsNew()
        {
            ProviderDiscoveryUri = "http://steamcommunity.com/openid/";
            Caption = "Steam";
            AuthenticationType = "Steam";
            CallbackPath = new PathString("/signin-openidsteam");
        }
    }

    /// <summary>
    /// OWIN middleware for authenticating users using an OpenID provider
    /// </summary>
    public sealed class SteamAuthenticationMiddlewareNew : OpenIDAuthenticationMiddlewareBase<SteamAuthenticationOptionsNew>
    {
        /// <summary>
        /// Initializes a <see cref="SteamAuthenticationMiddleware"/>
        /// </summary>
        /// <param name="next">The next middleware in the OWIN pipeline to invoke</param>
        /// <param name="app">The OWIN application</param>
        /// <param name="options">Configuration options for the middleware</param>
        public SteamAuthenticationMiddlewareNew(OwinMiddleware next, IAppBuilder app, SteamAuthenticationOptionsNew options)
            : base(next, app, options)
        { }

        protected override AuthenticationHandler<SteamAuthenticationOptionsNew> CreateSpecificHandler()
        {
            return new SteamAuthenticationHandlerNew(HTTPClient, Logger);
        }
    }

    internal sealed class SteamAuthenticationHandlerNew : OpenIDAuthenticationHandlerBase<SteamAuthenticationOptionsNew>
    {
        private readonly Regex _accountIDRegex = new Regex(@"^http(?:s)?://steamcommunity\.com/openid/id/(7[0-9]{15,25})$", RegexOptions.Compiled);

        private const string UserInfoUri = "http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}";

        public SteamAuthenticationHandlerNew(HttpClient httpClient, ILogger logger) : base(httpClient, logger)
        { }

        protected override void SetIdentityInformations(ClaimsIdentity identity, string claimedID, IDictionary<string, string> attributeExchangeProperties)
        {
            var accountIDMatch = _accountIDRegex.Match(claimedID);
            if (!accountIDMatch.Success) return;
            var accountID = accountIDMatch.Groups[1].Value;

            var getUserInfoTask = HTTPClient.GetStringAsync(string.Format(UserInfoUri, Options.ApplicationKey, accountID));
            getUserInfoTask.Wait();
            var userInfoRaw = getUserInfoTask.Result;
            dynamic userInfo = JsonConvert.DeserializeObject<dynamic>(userInfoRaw);
            identity.AddClaim(new Claim(ClaimTypes.Name, (string)userInfo.response.players[0].personaname, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
        }
    }
}
