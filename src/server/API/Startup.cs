using FluentPOS.Modules.Catalog;
using FluentPOS.Modules.Identity.Extensions;
using FluentPOS.Shared.Application.Extensions;
using FluentPOS.Shared.Infrastructure.Extensions;
using FluentPOS.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPOS.Bootstrapper
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddDistributedMemoryCache()
                .Configure<PersistenceSettings>(_config.GetSection("PersistenceSettings"))
                .AddIdentityModule(_config)
                .AddSharedInfrastructure(_config)
                .AddSharedApplication(_config)
                .AddCatalogModule(_config);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSharedInfrastructure();
        }
    }
}