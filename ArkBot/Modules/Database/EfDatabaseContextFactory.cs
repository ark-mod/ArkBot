using Autofac;
using Microsoft.EntityFrameworkCore.Design;

namespace ArkBot.Modules.Database
{
    public class EfDatabaseContextFactory : IDesignTimeDbContextFactory<EfDatabaseContext>
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

        public EfDatabaseContext CreateDbContext(string[] args)
        {
            return _scope != null ? _scope.Resolve<EfDatabaseContext>() : /*Only for Migrations*/ new EfDatabaseContext(@"Data Source=.\SQLExpress;Integrated Security=True;Database=ArkBot");
        }

        public EfDatabaseContext Create()
        {
            return _scope.Resolve<EfDatabaseContext>();
        }
    }
}
