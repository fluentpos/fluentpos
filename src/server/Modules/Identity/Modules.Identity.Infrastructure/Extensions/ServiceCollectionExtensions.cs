using FluentPOS.Modules.Identity.Core.Abstractions;
using FluentPOS.Modules.Identity.Core.Entities;
using FluentPOS.Modules.Identity.Core.Settings;
using FluentPOS.Modules.Identity.Infrastructure.Persistence;
using FluentPOS.Modules.Identity.Infrastructure.Services;
using FluentPOS.Shared.Application.Interfaces.Services;
using FluentPOS.Shared.Application.Interfaces.Services.Identity;
using FluentPOS.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentPOS.Modules.Identity.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddHttpContextAccessor()
                .AddScoped<ICurrentUser, CurrentUser>()
                .Configure<JwtSettings>(configuration.GetSection("JwtSettings"))
                .AddTransient<ITokenService, TokenService>()
                .AddTransient<IIdentityService, IdentityService>()
                .AddDatabaseContext<IdentityDbContext>()
                .AddIdentity<ExtendedIdentityUser, ExtendedIdentityRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<IdentityDbContext>().AddDefaultTokenProviders();
            services.AddTransient<IDatabaseSeeder, IdentityDbSeeder>();
            return services;
        }
    }
}