using ArkBot.Database.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Database
{
    public class EfDatabaseContext : DbContext, IEfDatabaseContext
    {
        public EfDatabaseContext() : base("EfDatabaseContext")
        {
        }

        public DbSet<Model.User> Users { get; set; }
        public DbSet<WildCreatureLog> WildCreatureLogs { get; set; }
        public DbSet<WildCreatureLogEntry> WildCreatureLogEntries { get; set; }
        public DbSet<TamedCreatureLogEntry> TamedCreatureLogEntries { get; set; }
        public DbSet<PlayedEntry> Played { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(new NonPublicColumnAttributeConvention());

            //modelBuilder.Entity<Database.Model.WildCreatureLogEntry>()
            //    .Property(x => x.IdsAsStrings).HasColumnType("ntext").HasMaxLength(int.MaxValue);

            modelBuilder.Entity<WildCreatureLogEntry>()
                .HasRequired(s => s.Log)
                .WithMany(s => s.Entries);

            modelBuilder.Entity<PlayedEntry>()
                .HasOptional(s => s.User)
                .WithMany(s => s.Played);
        }
    }
}
