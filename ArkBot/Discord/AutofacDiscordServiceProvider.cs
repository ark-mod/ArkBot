using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
