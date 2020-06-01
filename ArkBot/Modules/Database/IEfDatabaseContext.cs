using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Modules.Database
{
    public interface IEfDatabaseContext : System.IDisposable
    {
        DbSet<Model.Player> Players { get; set; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}