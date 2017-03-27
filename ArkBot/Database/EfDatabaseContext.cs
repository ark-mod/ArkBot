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
        public EfDatabaseContext(string nameOrConnectionString = null) : base(nameOrConnectionString)
        {
        }

        public DbSet<Model.User> Users { get; set; }
        public DbSet<WildCreatureLog> WildCreatureLogs { get; set; }
        public DbSet<WildCreatureLogEntry> WildCreatureLogEntries { get; set; }
        public DbSet<TamedCreatureLogEntry> TamedCreatureLogEntries { get; set; }
        public DbSet<PlayedEntry> Played { get; set; }
        public DbSet<Model.Vote> Votes { get; set; }
        public DbSet<UserVote> UserVotes { get; set; }

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

            modelBuilder.Entity<UserVote>()
                .HasRequired(s => s.User)
                .WithMany(s => s.Votes);

            modelBuilder.Entity<UserVote>()
                .HasRequired(s => s.Vote)
                .WithMany(s => s.Votes);

            //Setup votes tables (TPT: Table per Type)
            modelBuilder.Entity<BanVote>().ToTable("BanVotes");
            modelBuilder.Entity<UnbanVote>().ToTable("UnbanVotes");
            modelBuilder.Entity<SetTimeOfDayVote>().ToTable("SetTimeOfDayVotes");
            modelBuilder.Entity<DestroyWildDinosVote>().ToTable("DestroyWildDinosVotes");
            modelBuilder.Entity<UpdateServerVote>().ToTable("UpdateServerVotes");
            modelBuilder.Entity<RestartServerVote>().ToTable("RestartServerVotes");
        }
    }
}
