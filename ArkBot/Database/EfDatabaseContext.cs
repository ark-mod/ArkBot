using ArkBot.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace ArkBot.Database
{
    public class EfDatabaseContext : DbContext, IEfDatabaseContext
    {
        private readonly string _connectionString;

        public EfDatabaseContext(string nameOrConnectionString = null)
        {
            _connectionString = nameOrConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(_connectionString);

        public DbSet<Model.User> Users { get; set; }
        public DbSet<WildCreatureLog> WildCreatureLogs { get; set; }
        public DbSet<WildCreatureLogEntry> WildCreatureLogEntries { get; set; }
        public DbSet<TamedCreatureLogEntry> TamedCreatureLogEntries { get; set; }
        public DbSet<PlayedEntry> Played { get; set; }
        public DbSet<Model.Vote> Votes { get; set; }
        public DbSet<Model.BanVote> BanVotes { get; set; }
        public DbSet<Model.UnbanVote> UnbanVotes { get; set; }
        public DbSet<Model.SetTimeOfDayVote> SetTimeOfDayVotes { get; set; }
        public DbSet<Model.DestroyWildDinosVote> DestroyWildDinosVotes { get; set; }
        public DbSet<Model.UpdateServerVote> UpdateServerVotes { get; set; }
        public DbSet<Model.RestartServerVote> RestartServerVotes { get; set; }
        public DbSet<UserVote> UserVotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Add(new NonPublicColumnAttributeConvention());

            //modelBuilder.Entity<Database.Model.WildCreatureLogEntry>()
            //    .Property(x => x.IdsAsStrings).HasColumnType("ntext").HasMaxLength(int.MaxValue);

            modelBuilder.Entity<WildCreatureLogEntry>()
                .HasOne(s => s.Log)
                .WithMany(s => s.Entries);

            modelBuilder.Entity<PlayedEntry>()
                .HasOne(s => s.User)
                .WithMany(s => s.Played);

            modelBuilder.Entity<UserVote>()
                .HasOne(s => s.User)
                .WithMany(s => s.Votes);

            modelBuilder.Entity<UserVote>()
                .HasOne(s => s.Vote)
                .WithMany(s => s.Votes);

            //Setup votes tables (TPH: table-per-hierarchy)
            //TPT: Table per Type - not supported in ef core
            //modelBuilder.Entity<Vote>().ToTable("Vote");
            //modelBuilder.Entity<BanVote>().ToTable("BanVotes");
            //modelBuilder.Entity<UnbanVote>().ToTable("UnbanVotes");
            //modelBuilder.Entity<SetTimeOfDayVote>().ToTable("SetTimeOfDayVotes");
            //modelBuilder.Entity<DestroyWildDinosVote>().ToTable("DestroyWildDinosVotes");
            //modelBuilder.Entity<UpdateServerVote>().ToTable("UpdateServerVotes");
            //modelBuilder.Entity<RestartServerVote>().ToTable("RestartServerVotes");
        }
    }
}
