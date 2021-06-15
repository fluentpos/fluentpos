using FluentPOS.Modules.Catalog.Core.Entities;
using FluentPOS.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FluentPOS.Modules.Catalog.Infrastructure.Persistence
{
    public static class ModelBuilderExtensions
    {
        public static void ApplyCatalogConfiguration(this ModelBuilder builder, PersistenceSettings persistenceOptions)
        {
            // build model for MSSQL and Postgres

            builder.Entity<Product>(entity =>
            {
                entity.ToTable(name: "Products", "Catalog");

                if (persistenceOptions.UseMsSql)
                {
                    entity.Property(p => p.Price)
                        .HasColumnType("decimal(23, 2)");
                    entity.Property(p => p.Cost)
                        .HasColumnType("decimal(23, 2)");
                    entity.Property(p => p.AlertQuantity)
                        .HasColumnType("decimal(23, 2)");
                }
            });
        }
    }
}