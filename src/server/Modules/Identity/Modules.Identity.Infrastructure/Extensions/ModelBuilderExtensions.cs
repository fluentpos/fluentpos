using FluentPOS.Modules.Identity.Core.Entities;
using FluentPOS.Modules.Identity.Core.Entities.ExtendedAttributes;
using FluentPOS.Shared.Core.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace FluentPOS.Modules.Identity.Infrastructure.Extensions
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyIdentityConfiguration(this ModelBuilder builder, PersistenceSettings persistenceOptions)
        {
            // build model for MSSQL and Postgres

            builder.Entity<FluentUser>(entity =>
            {
                entity.ToTable(name: "Users");
            });
            builder.Entity<FluentRole>(entity =>
            {
                entity.ToTable(name: "Roles");
            });
            builder.Entity<FluentRoleClaim>(entity =>
            {
                entity.ToTable(name: "RoleClaims");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleClaims)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<IdentityUserRole<Guid>>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            builder.Entity<IdentityUserClaim<Guid>>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            builder.Entity<IdentityUserLogin<Guid>>(entity =>
            {
                entity.ToTable("UserLogins");
            });
            builder.Entity<IdentityUserToken<Guid>>(entity =>
            {
                entity.ToTable("UserTokens");
            });

            builder.Entity<UserExtendedAttribute>(entity =>
            {
                entity.ToTable("UserExtendedAttributes");

                if (persistenceOptions.UseMsSql)
                {
                    entity.Property(p => p.Decimal)
                        .HasColumnType("decimal(23, 2)");
                }
            });
            builder.Entity<RoleExtendedAttribute>(entity =>
            {
                entity.ToTable("RoleExtendedAttributes");

                if (persistenceOptions.UseMsSql)
                {
                    entity.Property(p => p.Decimal)
                        .HasColumnType("decimal(23, 2)");
                }
            });
        }
    }
}