using FluentPOS.Shared.Application.EventLogging;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace FluentPOS.Shared.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<EventLog> EventLogs { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}