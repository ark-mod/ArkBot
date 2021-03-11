using ArkBot.Modules.Application;
using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.WebApp.Hubs;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace ArkBot.Modules.WebApp
{
    public class WebAppStartup
    {
        private IConfig _config;

        public IWebHostEnvironment Env { get; private set; }
        public IConfigurationRoot Configuration { get; private set; }
        public ILifetimeScope AutofacContainer { get; private set; }

        public WebAppStartup(IWebHostEnvironment env)
        {
            Env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                // specifying AllowAnyOrigin and AllowCredentials is an insecure configuration and can result in cross-site request forgery.
                // it is not allowed in .NET Core but can be bypassed using .SetIsOriginAllowed(isOriginAllowed: _ => true)
                // todo: we need this for testing, but maybe not in release builds (could introduce an option for allowed cors origins)
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder
                        .SetIsOriginAllowed(isOriginAllowed: _ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                    });
            });

            // PropertyNamingPolicy = null important to preserve case of property names
            // Use the default property (Pascal) casing
            //services.AddControllers().AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                // Use the default property (Pascal) casing
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

            services.AddRouting(options =>
            {
            });

            services.AddResponseCompression(options =>
            {
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                //options.EventsType = typeof(CustomCookieAuthenticationEvents);
            })
            .AddSteam(options =>
            {
                options.ApplicationKey = _config.SteamApiKey;
            });

            services.AddAuthorization();

            //services.AddHttpsRedirection(options =>
            //{
            //    options.HttpsPort = IPEndPoint.TryParse(_config.WebApp.IPEndpoint, out var ipEndpoint) ? ipEndpoint.Port : 443;
            //});
        }

        public void Configure(IApplicationBuilder app)
        {
            AutofacContainer = app.ApplicationServices.GetAutofacRoot();
            _config = AutofacContainer.Resolve<IConfig>();

            //TODO [.NET Core]: Not sure if there is a better way to do this. We need to use a child lifetime scope and thus can't register the hub context to be accessible from the root container.
            var notificationMangager = AutofacContainer.Resolve<NotificationManager>();
            var hubContext = app.ApplicationServices.GetService<Microsoft.AspNetCore.SignalR.IHubContext<ServerUpdateHub, IServerUpdateClient>>();
            notificationMangager.Setup(hubContext);

            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true
            });

            //app.UseDirectoryBrowser();

            //app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = new[] { "index.html" } } );

            //app.UseHsts();

            //if (_config.WebApp.Ssl.Enabled && _config.WebApp.Ssl.UseHttpsRedirect) app.UseHttpsRedirection();

            // use https redirection for remote requests (local requests could be from link and we want those to use http)
            if (_config.WebApp.Ssl.Enabled && _config.WebApp.Ssl.UseHttpsRedirect)
            {
                var options = new RewriteOptions();
                options.Rules.Add(new RedirectToHttpsRuleCustom
                {
                    StatusCode = StatusCodes.Status302Found,
                    SSLPort = IPEndPoint.TryParse(_config.WebApp.IPEndpoint, out var ipEndpoint) ? ipEndpoint.Port : 443
                });
                app.UseWhen(HttpContextFilters.IsRemoteRequest, configuration => configuration.UseRewriter(options));
            }

            app.UseRouting(); // must appear before UseCors()

            app.UseCors();

            app.UseResponseCompression();

            //app.UseWebSockets(new WebSocketOptions { });

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                Secure = _config.WebApp.Ssl?.Enabled == true ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest,
                MinimumSameSitePolicy = _config.WebApp.Ssl?.Enabled == true ? SameSiteMode.None : SameSiteMode.Lax
            });

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                var sendSpaIndexFile = new Action<HttpContext>(context =>
                {
                    var filePath = @"WebApp\index.html";
                    if (!File.Exists(filePath))
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }

                    var contents = File.ReadAllText(filePath);
                    var obj = new
                    {
                        webapi = new
                        {
                        },
                        webapp = new
                        {
                            defaultTheme = _config.WebApp.DefaultTheme.ToString(),
                            topMenu = _config.WebApp.TopMenu,
                            useCustomCssFile = !string.IsNullOrEmpty(_config.WebApp.CustomCssFilePath)
                        }
                    };
                    var json = JsonConvert.SerializeObject(obj, Formatting.None);
                    var js = $"var config = {json};";
                    contents = contents.Replace("/*[[config]]*/", js);

                    context.Response.ContentType = "text/html; charset=utf-8";
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    context.Response.WriteAsync(contents);
                });

                var app2 = endpoints.CreateApplicationBuilder();
                app2.Use(async (context, next) =>
                {
                    // could use the static file context which supports range requests, last modified, etags etc. (but it is internal)
                    // https://github.com/dotnet/aspnetcore/blob/19d2f6124f5d04859e350d1f5a01e994e14ef1ce/src/Middleware/StaticFiles/src/StaticFileContext.cs
                    sendSpaIndexFile(context);

                    //var responseCompressionFeature = context.Features.Get<IHttpsCompressionFeature>();
                    //if (responseCompressionFeature != null)
                    //{
                    //    responseCompressionFeature.Mode = HttpsCompressionMode.Compress;
                    //}
                });

                endpoints.MapFallback(app2.Build());

                if (!string.IsNullOrEmpty(_config.WebApp.CustomCssFilePath) && File.Exists(_config.WebApp.CustomCssFilePath))
                {
                    endpoints.MapGet("/custom.css", context =>
                    {
                        context.Response.ContentType = "text/css; charset=utf-8";
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        return context.Response.WriteAsync(File.ReadAllText(_config.WebApp.CustomCssFilePath));
                    });
                }

                endpoints.MapControllers();

                endpoints.MapHub<ServerUpdateHub>("/hub", options =>
                {
                });

                endpoints.MapHub<ArkBotLinkHub>("/arkbotlink", options =>
                {
                });
            });
        }
    }

    public static class HttpContextFilters
    {
        public static bool IsRemoteRequest(HttpContext context)
        {
            if (context.Connection.RemoteIpAddress.Equals(context.Connection.LocalIpAddress)) return false;
            if (IPAddress.IsLoopback(context.Connection.RemoteIpAddress)) return false;
            return true;
        }
    }

    internal class RedirectToHttpsRuleCustom : IRule
    {
        public int? SSLPort { get; set; }
        public int StatusCode { get; set; }

        private static readonly Action<Microsoft.Extensions.Logging.ILogger, Exception> _redirectedToHttps;

        static RedirectToHttpsRuleCustom()
        {
            _redirectedToHttps = Microsoft.Extensions.Logging.LoggerMessage.Define(
                            Microsoft.Extensions.Logging.LogLevel.Information,
                            new Microsoft.Extensions.Logging.EventId(8, "RedirectedToHttps"),
                            "Request redirected to HTTPS");
        }

        public virtual void ApplyRule(RewriteContext context)
        {
            if (!context.HttpContext.Request.IsHttps)
            {
                var host = context.HttpContext.Request.Host;
                if (SSLPort.HasValue && SSLPort.Value > 0)
                {
                    // a specific SSL port is specified
                    host = new HostString(host.Host, SSLPort.Value);
                }
                else
                {
                    // clear the port
                    host = new HostString(host.Host);
                }

                var req = context.HttpContext.Request;
                var newUrl = new StringBuilder().Append("https://").Append(host).Append(req.PathBase).Append(req.Path).Append(req.QueryString);
                var response = context.HttpContext.Response;
                response.StatusCode = StatusCode;
                response.Headers[HeaderNames.Location] = newUrl.ToString();
                context.Result = RuleResult.EndResponse;
                //context.Logger.RedirectedToHttps();
                _redirectedToHttps(context.Logger, null);
            }
        }
    }
}
