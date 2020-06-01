using Autofac;
using System;

namespace ArkBot.Modules.Discord
{
    public class AutofacDiscordServiceProvider : IServiceProvider
    {
        private readonly ILifetimeScope _scope;

        public AutofacDiscordServiceProvider(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public object GetService(Type serviceType)
        {
            return _scope.Resolve(serviceType);
        }
    }
}
