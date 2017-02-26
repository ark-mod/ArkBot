namespace ArkBot.Migrations
{
    using Autofac;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

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

            if (context.Users.Count() > 0) return;
            using (var ctx = new ArkBot.Database.DatabaseContext(Constants.DatabaseConnectionString))
            {
                var users = ctx.Users.ToArray();
                foreach(var user in users)
                {
                    context.Users.Add(new ArkBot.Database.Model.User { SteamId = (long)user.SteamId, DiscordId = (long)user.DiscordId, RealName = user.RealName, SteamDisplayName = user.SteamDisplayName });
                }

                context.SaveChanges();
            }
        }
    }
}
