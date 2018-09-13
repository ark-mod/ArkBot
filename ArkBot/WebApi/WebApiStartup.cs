using ArkBot.ViewModel;
using Autofac;
using Autofac.Core;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.IO;
using Microsoft.Owin.Security;
using Owin.Security.Providers.Steam;
using Microsoft.Owin.Security.OAuth;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.Cookies;
using ArkBot.Configuration.Model;
using ArkBot.OpenID;

namespace ArkBot.WebApi
{
    public class WebApiStartup
    {
        // This code configures Web API.
        public void Configuration(IAppBuilder appBuilder, IConfig _config, IContainer container, HttpConfiguration config)
        {
            // Configure Web API for self-host. 
            config.Routes.MapHttpRoute(
                name: "DefaultAuth",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: new { controller = "authentication" }
            );
            config.Routes.MapHttpRoute(
                name: "DefaultAdminister",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: new { controller = "administer" }
            );
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            config.Formatters.Add(new BrowserJsonFormatter());
            config.Services.Replace(typeof(System.Web.Http.ExceptionHandling.IExceptionLogger), new WebApiExceptionLogger());

            var hubConfig = new HubConfiguration { EnableDetailedErrors = true };
            hubConfig.Resolver = container.Resolve<IDependencyResolver>();

            appBuilder.UseAutofacMiddleware(container);
            appBuilder.UseAutofacWebApi(config);
            appBuilder.UseCompressionModule();
            appBuilder.UseCors(CorsOptions.AllowAll);

            appBuilder.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookie",
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                CookieSecure = _config.Ssl?.Enabled == true ? CookieSecureOption.Always : CookieSecureOption.SameAsRequest
            });

            appBuilder.SetDefaultSignInAsAuthenticationType("ExternalCookie");
            appBuilder.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "ExternalCookie",
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Passive,
            });

            appBuilder.UseSteamAuthenticationNew(applicationKey: _config.SteamApiKey);

            appBuilder.UseWebApi(config);
            appBuilder.MapSignalR(hubConfig);
            //appBuilder.UseFileServer(new FileServerOptions
            //{
            //    FileSystem = new PhysicalFileSystem(@"WebApi\Static\"),
            //    RequestPath = new PathString("/app"),
            //});
        }
    }

    public class BrowserJsonFormatter : JsonMediaTypeFormatter
    {
        public BrowserJsonFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            //SerializerSettings.Formatting = Formatting.Indented;
        }

        public override void SetDefaultContentHeaders(Type type, HttpContentHeaders headers, MediaTypeHeaderValue mediaType)
        {
            base.SetDefaultContentHeaders(type, headers, mediaType);
            headers.ContentType = new MediaTypeHeaderValue("application/json");
        }
    }
}
