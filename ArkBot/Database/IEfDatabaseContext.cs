using System.Data.Entity;
using ArkBot.Database.Model;
using System.Threading.Tasks;
using System.Threading;
using System.Data.Entity.Infrastructure;

namespace ArkBot.Database
{
    public interface IEfDatabaseContext : System.IDisposable
    {
        DbSet<Model.User> Users { get; set; }
        DbSet<WildCreatureLogEntry> WildCreatureLogEntries { get; set; }
        DbSet<WildCreatureLog> WildCreatureLogs { get; set; }
        DbSet<TamedCreatureLogEntry> TamedCreatureLogEntries { get; set; }
        DbSet<PlayedEntry> Played { get; set; }
        DbSet<Model.Vote> Votes { get; set; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        DbEntityEntry Entry(object entity);

        System.Data.Entity.Database Database { get; }
    }
}