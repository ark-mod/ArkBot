using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Database
{
    //todo: this is a really crappy factory
    public class DatabaseContextFactory<T>
        where T: IDisposable
    {
        private ILifetimeScope _scope;

        public DatabaseContextFactory(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public T Create()
        {
            return _scope.Resolve<T>();
        }
    }
}
