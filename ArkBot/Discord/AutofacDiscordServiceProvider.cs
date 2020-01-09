using System;
using Autofac;

namespace ArkBot.Discord
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
