namespace ArkBot.Migrations
{
    using Autofac;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    /* NOTE that x64 binary build is not supported by these commands in Package Manager Console
     * ----------------------------------------------------------------------------------------
     * Add new Migration: $ Add-Migration <name>
     * Update database: $ Update-Database
     * Update database to a specific migration: $ Update-Database –TargetMigration: <name>
     * Update database to base: $ Update-Database –TargetMigration: $InitialDatabase
     */

    internal sealed class Configuration : DbMigrationsConfiguration<ArkBot.Database.EfDatabaseContext>
    {
        public IConstants Constants { get; set; }

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ArkBot.Database.EfDatabaseContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
