using FluentPOS.Shared.Application.EventLogging;
using FluentPOS.Shared.Application.Interfaces;
using FluentPOS.Shared.Application.Interfaces.Services;
using FluentPOS.Shared.Application.Settings;
using FluentPOS.Shared.Infrastructure.Controllers;
using FluentPOS.Shared.Infrastructure.EventLogging;
using FluentPOS.Shared.Infrastructure.Middlewares;
using FluentPOS.Shared.Infrastructure.Persistence;
using FluentPOS.Shared.Infrastructure.Persistence.Postgres;
using FluentPOS.Shared.Infrastructure.Services;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Bootstrapper")]

namespace FluentPOS.Shared.Infrastructure.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration config)
        {
            services.AddTransient<IUploadService, UploadService>();
            services.AddTransient<IMailService, SmtpMailService>();
            services.AddScoped<IJobService, HangfireService>();
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            return services;
        }

        public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services
                .AddDatabaseContext<ApplicationDbContext>()
                .AddScoped<IApplicationDbContext>(provider => provider.GetService<ApplicationDbContext>());

            services.AddScoped<IEventLogger, EventLogger>();
            services.AddControllers()
                .ConfigureApplicationPartManager(manager =>
                {
                    manager.FeatureProviders.Add(new InternalControllerFeatureProvider());
                });
            services.AddApplicationLayer(config);
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddPostgres();
            services.AddHangfireServer();
            services.AddSingleton<GlobalExceptionHandler>();
            services.AddSwaggerDocumentation();
            return services;
        }

        internal static T GetOptions<T>(this IServiceCollection services, string sectionName) where T : new()
        {
            using var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var section = configuration.GetSection(sectionName);
            var options = new T();
            section.Bind(options);

            return options;
        }

        private static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            return services.AddSwaggerGen(c =>
            {
                c.IncludeXmlComments($@"{AppDomain.CurrentDomain.BaseDirectory}\Bootstrapper.xml");
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "FluentPOS.API",
                    License = new OpenApiLicense()
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        }, new List<string>()
                    },
                });
            });
        }
    }
}