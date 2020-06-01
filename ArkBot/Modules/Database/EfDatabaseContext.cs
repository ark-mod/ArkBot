using ArkBot.Modules.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace ArkBot.Modules.Database
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

        public DbSet<Model.Player> Players { get; set; }
        public DbSet<Model.ChatMessage> ChatMessages { get; set; }
        public DbSet<Model.LoggedLocation> LoggedLocations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Player>()
                .HasMany(s => s.ChatMessages)
                .WithOne(s => s.Player);

            modelBuilder.Entity<Player>()
                .HasMany(s => s.LoggedLocations)
                .WithOne(s => s.Player);

            modelBuilder.Entity<Player>()
                .HasIndex(e => new { e.IsOnline, e.LastServerKey });

            modelBuilder.Entity<LoggedLocation>()
                .HasIndex(e => e.At);

            modelBuilder.Entity<ChatMessage>()
                .HasIndex(e => new { e.ServerKey, e.At });
        }
    }
}
