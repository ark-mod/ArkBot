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
using Nancy;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Nancy.Conventions;
using System.IO;
using Nancy.Bootstrappers.Autofac;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using ArkBot.Configuration.Model;

namespace ArkBot.WebApp
{
    public class WebAppStartup
    {
        // This code configures the Web App. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder, IConfig _config, IContainer container, HttpConfiguration config)
        {
            // Configure Web App for self-host. 
            appBuilder.UseAutofacMiddleware(container);
            appBuilder.UseCompressionModule();
            appBuilder.UseCors(CorsOptions.AllowAll);
            appBuilder.UseNancy(new Nancy.Owin.NancyOptions
            {
                Bootstrapper = new CustomBootstrapper(container)
            });
            //appBuilder.UseFileServer(new FileServerOptions
            //{
            //    FileSystem = new PhysicalFileSystem(@"WebApi\Static\"),
            //    RequestPath = new PathString("/app"),
            //});
        }
    }

    public class CustomBootstrapper : AutofacNancyBootstrapper, IRootPathProvider
    {
        private IContainer _container;

        public CustomBootstrapper(IContainer container)
        {
            _container = container;
        }

        protected override ILifetimeScope GetApplicationContainer()
        {
            return _container;
        }
      
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            //nancyConventions.StaticContentsConventions.Add(
            //    StaticContentConventionBuilder.AddDirectory("/", @"")
            //);
        }

        protected override IRootPathProvider RootPathProvider
        {
            get { return this; }
        }

        public string GetRootPath()
        {
            return System.IO.Path.GetFullPath(@"WebApp");
        }
    }
  
    public class SinglePageApplicationModule : NancyModule
    {
        private IConfig _config;

        public SinglePageApplicationModule(IConfig config)
        {
            _config = config;

            var getIndex = new Func<Response>(() =>
            {
                var filePath = @"WebApp\index.html";
                if (!File.Exists(filePath)) return HttpStatusCode.NotFound;

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
                        defaultTheme = _config.WebApp.DefaultTheme.ToString(),
                        topMenu = _config.WebApp.TopMenu,
                        useCustomCssFile = !string.IsNullOrEmpty(_config.WebApp.CustomCssFilePath)
                    }
                };
                var json = JsonConvert.SerializeObject(obj, Formatting.None);
                var js = $"var config = {json};";
                contents = contents.Replace("/*[[config]]*/", js);

                return Response.AsText(contents, "text/html; charset=utf-8");
            });

            Get[""] = _ => getIndex();

            Get[@"^(?<path>.*)$"] = parameters =>
            {
              if (parameters["path"].Value.Equals("custom.css") && !string.IsNullOrEmpty(_config.WebApp.CustomCssFilePath) && File.Exists(_config.WebApp.CustomCssFilePath))
              {
                return Response.AsText(File.ReadAllText(_config.WebApp.CustomCssFilePath), "text/css");
              }

              if (File.Exists(Path.Combine(Response.RootPath, parameters["path"].Value))) return Response.AsFile((string)parameters["path"].Value);
                return getIndex();
            };
        }
    }
}
