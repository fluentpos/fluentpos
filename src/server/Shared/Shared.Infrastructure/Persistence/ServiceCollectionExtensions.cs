using FluentPOS.Shared.Infrastructure.Extensions;
using FluentPOS.Shared.Infrastructure.Persistence.MSSQL;
using FluentPOS.Shared.Infrastructure.Persistence.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPOS.Shared.Infrastructure.Persistence
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseContext<T>(this IServiceCollection services) where T : DbContext
        {
            var options = services.GetOptions<PersistenceSettings>("PersistenceSettings");
            if (options.UsePostgres)
            {
                services.AddPostgres<T>();
            }
            else if (options.UseMsSql)
            {
                services.AddMSSQL<T>();
            }
            return services;
        }
    }
}