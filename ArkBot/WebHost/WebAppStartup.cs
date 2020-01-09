using Autofac;
using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ArkBot.Configuration.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using ArkBot.WebApi.Hubs;
using ArkBot.OpenID;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;

namespace ArkBot.WebHost
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
            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(env.ContentRootPath)
            //    //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            //    .AddEnvironmentVariables();
            //this.Configuration = builder.Build();
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

            //services.AddMvc(options =>
            //{
            //    options.EnableEndpointRouting = false;
            //});

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
                //options.DefaultChallengeScheme = SteamDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                //options.EventsType = typeof(CustomCookieAuthenticationEvents);
            })
            .AddSteam(options =>
            {
            });

            services.AddAuthorization();
        }

        public void Configure(IApplicationBuilder app)
        {
            AutofacContainer = app.ApplicationServices.GetAutofacRoot();
            _config = AutofacContainer.Resolve<IConfig>();

            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                //FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "WebApp")),
                //RequestPath = "/WebApp",
                ServeUnknownFileTypes = true
            });

            //app.UseDirectoryBrowser();

            //app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = new[] { "index.html" } } );

            //app.UseHsts();

            //app.UseHttpsRedirection();

            app.UseRouting(); // must appear before UseCors()

            app.UseCors();

            app.UseResponseCompression();

            //app.UseWebSockets(new WebSocketOptions { });

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                Secure = _config.Ssl?.Enabled == true ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest
            });

            app.UseAuthentication();

            app.UseAuthorization();

            //app.UseMvc(
            //routes => {
            //    routes.MapRoute(
            //        name: "api",
            //        template: "api/{controller}/{action}/{id?}");

            //    // ## commented out since I don't want to use MVC to serve my index.    
            //    // routes.MapRoute(
            //    //     name:"spa-fallback", 
            //    //     template: "{*anything}", 
            //    //     defaults: new { controller = "Home", action = "Index" });
            //});


            //// ## this serves my index.html from the wwwroot folder when 
            //// ## a route not containing a file extension is not handled by MVC.  
            //// ## If the route contains a ".", a 404 will be returned instead.
            //app.MapWhen(context => context.Response.StatusCode == 404 && !Path.HasExtension(context.Request.Path.Value),
            //            branch => {
            //                branch.Use((context, next) => {
            //                    context.Request.Path = new PathString("/index.html");
            //                    Console.WriteLine("Path changed to:" + context.Request.Path.Value);
            //                    return next();
            //                });

            //                branch.UseStaticFiles();
            //            });


            //// Handle Lets Encrypt Route(before MVC processing!)
            //app.UseRouter(r =>
            //{
            //    r.MapGet(".well-known/acme-challenge/{id}", async (request, response, routeData) =>
            //    {
            //        var id = routeData.Values["id"] as string;
            //        var file = Path.Combine(env.WebRootPath, ".well-known", "acme-challenge", id);
            //        await response.SendFileAsync(file);
            //    });
            //});

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
                    var portStr = new Regex(@":(?<port>\d+)(?:/|$)").Match(_config.WebApiListenPrefix)?.Groups["port"].Value;
                    var success = int.TryParse(portStr, out var port);
                    var obj = new
                    {
                        webapi = new
                        {
                            port = success ? port : (int?)null
                        },
                        webapp = new
                        {
                            defaultTheme = _config.WebApp.DefaultTheme.ToString()
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

                //endpoints.MapFallbackToFile("index.html", new StaticFileOptions
                //{
                //    OnPrepareResponse = x =>
                //    {
                //        var httpContext = x.Context;
                //        var path = httpContext.Request.RouteValues["path"];
                //        // now you get the original request path
                //    }
                //});

                //endpoints.MapGet("/", context =>
                //{
                //    return context.Response.SendFileAsync("webapp/index.html");
                //    //var stream = File.OpenRead("webapp/index.html");
                //    //return new FileStreamResult(stream, "application/octet-stream");
                //});

                endpoints.MapControllers();
                //endpoints.MapControllerRoute("default", "api/{controller}/{action=Get}/{id?}");

                //endpoints.MapGet("/", context => context.Response.Write("Hello world"));

                endpoints.MapHub<ServerUpdateHub>("/hub", options =>
                {
                });

                //endpoints.MapControllerRoute(
                //    "DefaultAuth",
                //    "api/{controller}/{action}/{id?}",
                //    constraints: new { controller = "authentication" }
                //);
                //endpoints.MapControllerRoute(
                //    "DefaultAdminister",
                //    "api/{controller}/{action}/{id?}",
                //    constraints: new { controller = "administer" }
                //);
                //endpoints.MapControllerRoute(
                //    "DefaultApi",
                //    "api/{controller}/{id?}"
                //);

                //endpoints.MapControllerRoute("test", "api/{controller}/{action=Get}/{id?}");

                //endpoints.MapDefaultControllerRoute();

                //endpoints.MapControllerRoute(
                //name: "default",
                //pattern: "{controller=Home}/{action=Index}/{id?}");
            });


            // FROM .NET 4.6.1
            //            appBuilder.UseCompressionModule();
            //            appBuilder.UseCors(CorsOptions.AllowAll);

            //            appBuilder.UseCookieAuthentication(new CookieAuthenticationOptions
            //            {
            //                AuthenticationType = "Cookie",
            //                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
            //                CookieSecure = _config.Ssl?.Enabled == true ? CookieSecureOption.Always : CookieSecureOption.SameAsRequest
            //            });

            //            appBuilder.SetDefaultSignInAsAuthenticationType("ExternalCookie");
            //            appBuilder.UseCookieAuthentication(new CookieAuthenticationOptions
            //            {
            //                AuthenticationType = "ExternalCookie",
            //                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Passive,
            //            });

            //            appBuilder.UseSteamAuthenticationNew(applicationKey: _config.SteamApiKey);
        }
    }

    //public class WebAppStartup
    //{
    //    // This code configures the Web App. The Startup class is specified as a type
    //    // parameter in the WebApp.Start method.
    //    public void Configuration(IAppBuilder appBuilder, IConfig _config, IContainer container, HttpConfiguration config)
    //    {
    //        // Configure Web App for self-host. 
    //        appBuilder.UseAutofacMiddleware(container);
    //        appBuilder.UseCompressionModule();
    //        appBuilder.UseCors(CorsOptions.AllowAll);
    //        appBuilder.UseNancy(new Nancy.Owin.NancyOptions
    //        {
    //            Bootstrapper = new CustomBootstrapper(container)
    //        });
    //        //appBuilder.UseFileServer(new FileServerOptions
    //        //{
    //        //    FileSystem = new PhysicalFileSystem(@"WebApi\Static\"),
    //        //    RequestPath = new PathString("/app"),
    //        //});
    //    }
    //}

    //public class CustomBootstrapper : AutofacNancyBootstrapper, IRootPathProvider
    //{
    //    private IContainer _container;

    //    public CustomBootstrapper(IContainer container)
    //    {
    //        _container = container;
    //    }

    //    protected override ILifetimeScope GetApplicationContainer()
    //    {
    //        return _container;
    //    }

    //    protected override void ConfigureConventions(NancyConventions nancyConventions)
    //    {
    //        base.ConfigureConventions(nancyConventions);

    //        //nancyConventions.StaticContentsConventions.Add(
    //        //    StaticContentConventionBuilder.AddDirectory("/", @"")
    //        //);
    //    }

    //    protected override IRootPathProvider RootPathProvider
    //    {
    //        get { return this; }
    //    }

    //    public string GetRootPath()
    //    {
    //        return System.IO.Path.GetFullPath(@"WebApp");
    //    }
    //}

    //public class SinglePageApplicationModule : NancyModule
    //{
    //    private IConfig _config;

    //    public SinglePageApplicationModule(IConfig config)
    //    {
    //        _config = config;

    //        Get[""] = _ =>
    //        {
    //            return Response.AsFile(@"index.html");
    //        };
    //        Get[@"^(?<path>.*)$"] = parameters =>
    //        {
    //            if (parameters["path"].Value.Equals("config.js"))
    //            {
    //                var portStr = new Regex(@":(?<port>\d+)(?:/|$)").Match(_config.WebApiListenPrefix)?.Groups["port"].Value;
    //                var success = int.TryParse(portStr, out var port);
    //                var obj = new {
    //                  webapi = new {
    //                    port = success ? port : (int?)null
    //                  },
    //                  webapp = new {
    //                    defaultTheme = _config.WebApp.DefaultTheme.ToString()
    //                  }
    //                };
    //                var json = JsonConvert.SerializeObject(obj, Formatting.None);
    //                var js = $"var config = {json};";
    //                return Response.AsText(js, "application/javascript");
    //            }
    //            if (File.Exists(Path.Combine(Response.RootPath, parameters["path"].Value))) return Response.AsFile((string)parameters["path"].Value);
    //            return Response.AsFile(@"index.html");
    //        };
    //    }
    //}
}
