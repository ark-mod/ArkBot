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

        /// <summary>
        /// Dont use this constructor. Only for Migrations.
        /// </summary>
        public EfDatabaseContextFactory()
        {
        }

        public EfDatabaseContextFactory(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public EfDatabaseContext Create()
        {
            return _scope != null ? _scope.Resolve<EfDatabaseContext>() : /*Only for Migrations*/ new EfDatabaseContext(new Constants().DatabaseConnectionString);
        }
    }
}
