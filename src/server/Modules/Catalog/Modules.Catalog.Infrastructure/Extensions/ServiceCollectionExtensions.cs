using FluentPOS.Modules.Catalog.Core.Abstractions;
using FluentPOS.Modules.Catalog.Infrastructure.Persistence;
using FluentPOS.Shared.Application.Interfaces.Services;
using FluentPOS.Shared.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPOS.Modules.Catalog.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services)
        {
            services
                 .AddDatabaseContext<CatalogDbContext>()
                 .AddScoped<ICatalogDbContext>(provider => provider.GetService<CatalogDbContext>());
            services.AddTransient<IDatabaseSeeder, CatalogDbSeeder>();
            return services;
        }
    }
}