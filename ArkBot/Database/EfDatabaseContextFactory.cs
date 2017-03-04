using Autofac;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Database
{
    public class EfDatabaseContextFactory : IDbContextFactory<EfDatabaseContext>
    {
        private ILifetimeScope _scope;

        public EfDatabaseContextFactory(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public EfDatabaseContext Create()
        {
            return _scope.Resolve<EfDatabaseContext>();
        }
    }
}
