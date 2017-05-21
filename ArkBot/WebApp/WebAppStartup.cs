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

namespace ArkBot.WebApp
{
    public class WebAppStartup
    {
        // This code configures the Web App. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web App for self-host. 
            appBuilder.UseAutofacMiddleware(Workspace.Container);
            appBuilder.UseCompressionModule();
            appBuilder.UseCors(CorsOptions.AllowAll);
            appBuilder.UseNancy(new Nancy.Owin.NancyOptions
            {
                Bootstrapper = new CustomBootstrapper()
            });
            //appBuilder.UseFileServer(new FileServerOptions
            //{
            //    FileSystem = new PhysicalFileSystem(@"WebApi\Static\"),
            //    RequestPath = new PathString("/app"),
            //});
        }
    }

    public class CustomBootstrapper : DefaultNancyBootstrapper, IRootPathProvider
    {
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
        public SinglePageApplicationModule()
        {
            Get[@"^(?<path>.*)$"] = parameters =>
            {
                if (File.Exists(Path.Combine(Response.RootPath, parameters["path"].Value))) return Response.AsFile((string)parameters["path"].Value);
                return Response.AsFile(@"index.html");
            };
        }
    }
}
