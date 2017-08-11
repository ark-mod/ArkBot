using System;
using System.Threading.Tasks;
using ArkBot.ViewModel;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

namespace ArkBot.WebApp
{
    public class WebAppRedirectStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            //appBuilder.UseAutofacMiddleware(Workspace.Container);
            //appBuilder.UseCompressionModule();
            //appBuilder.UseCors(CorsOptions.AllowAll);
            appBuilder.Use<RedirectMiddleware>(Workspace.Instance._config.BotUrl);
        }
    }

    public class RedirectMiddleware : OwinMiddleware
    {
        private string _botUrl;

        public RedirectMiddleware(OwinMiddleware next, string botUrl) : base(next)
        {
            _botUrl = botUrl;
        }

        public override Task Invoke(IOwinContext context)
        {
            context.Response.StatusCode = 301;
            context.Response.Headers.Set("Location", _botUrl);

            return Task.CompletedTask;
        }
    }
}
