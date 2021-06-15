using FluentPOS.Modules.Identity.Core.Entities;
using FluentPOS.Modules.Identity.Infrastructure.Extensions;
using FluentPOS.Shared.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FluentPOS.Modules.Identity.Infrastructure.Persistence
{
    public class IdentityDbContext : IdentityDbContext<ExtendedIdentityUser, ExtendedIdentityRole, int>
    {
        private readonly PersistenceSettings _persistenceOptions;

        public IdentityDbContext(DbContextOptions<IdentityDbContext> options, IOptions<PersistenceSettings> persistenceOptions) : base(options)
        {
            _persistenceOptions = persistenceOptions.Value;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
            modelBuilder.ApplyIdentityConfiguration(_persistenceOptions);
        }
    }
}