using System;
using System.Threading;
using System.Threading.Tasks;
using FluentPOS.Modules.Identity.Core.Abstractions;
using FluentPOS.Modules.Identity.Core.Entities;
using FluentPOS.Modules.Identity.Core.Entities.ExtendedAttributes;
using FluentPOS.Modules.Identity.Infrastructure.Extensions;
using FluentPOS.Shared.Core.Domain;
using FluentPOS.Shared.Core.EventLogging;
using FluentPOS.Shared.Core.Interfaces;
using FluentPOS.Shared.Core.Settings;
using FluentPOS.Shared.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FluentPOS.Modules.Identity.Infrastructure.Persistence
{
    public sealed class IdentityDbContext : IdentityDbContext<FluentUser, FluentRole, Guid, IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>, FluentRoleClaim, IdentityUserToken<Guid>>,
        IIdentityDbContext,
        IModuleDbContext,
        IExtendedAttributeDbContext<Guid, FluentUser, UserExtendedAttribute>,
        IExtendedAttributeDbContext<Guid, FluentRole, RoleExtendedAttribute>
    {
        private readonly IMediator _mediator;
        private readonly IEventLogger _eventLogger;
        private readonly PersistenceSettings _persistenceOptions;

        internal string Schema => "Identity";

        public IdentityDbContext(
            DbContextOptions<IdentityDbContext> options,
            IOptions<PersistenceSettings> persistenceOptions,
            IMediator mediator,
            IEventLogger eventLogger)
                : base(options)
        {
            _mediator = mediator;
            _eventLogger = eventLogger;
            _persistenceOptions = persistenceOptions.Value;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);
            modelBuilder.Ignore<Event>();
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
            modelBuilder.ApplyIdentityConfiguration(_persistenceOptions);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await this.SaveChangeWithPublishEventsAsync(_eventLogger, _mediator, cancellationToken);
        }

        public override int SaveChanges()
        {
            return this.SaveChangeWithPublishEvents(_eventLogger, _mediator);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            return SaveChanges();
        }

        DbSet<FluentUser> IExtendedAttributeDbContext<Guid, FluentUser, UserExtendedAttribute>.GetEntities() => Users;

        DbSet<UserExtendedAttribute> IExtendedAttributeDbContext<Guid, FluentUser, UserExtendedAttribute>.ExtendedAttributes { get; set; }

        DbSet<FluentRole> IExtendedAttributeDbContext<Guid, FluentRole, RoleExtendedAttribute>.GetEntities() => Roles;

        DbSet<RoleExtendedAttribute> IExtendedAttributeDbContext<Guid, FluentRole, RoleExtendedAttribute>.ExtendedAttributes { get; set; }
    }
}