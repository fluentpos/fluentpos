using FluentPOS.Modules.Catalog.Core.Entities;
using FluentPOS.Shared.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FluentPOS.Modules.Catalog.Infrastructure.Persistence
{
    public class CatalogDbSeeder : IDatabaseSeeder
    {
        private readonly ILogger<CatalogDbSeeder> _logger;
        private readonly CatalogDbContext _db;

        public CatalogDbSeeder(ILogger<CatalogDbSeeder> logger, CatalogDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public void Initialize()
        {
            try
            {
                AddBrands();
                AddCategories();
                AddProducts();
                _db.SaveChanges();
            }
            catch (System.Exception)
            {
                _logger.LogError("An error occurred while seeding Catalog data.");
            }
        }

        private void AddBrands()
        {
            Task.Run(async () =>
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!_db.Brands.Any())
                {
                    var brandData = await File.ReadAllTextAsync(path + @"/Persistence/SeedData/brands.json");
                    var brands = JsonConvert.DeserializeObject<List<Brand>>(brandData);

                    if (brands != null)
                    {
                        foreach (var brand in brands)
                        {
                            _db.Brands.Add(brand);
                        }
                    }

                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Seeded Brands.");
                }
            }).GetAwaiter().GetResult();
        }

        private void AddCategories()
        {
            Task.Run(async () =>
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!_db.Categories.Any())
                {
                    var categoryData = await File.ReadAllTextAsync(path + @"/Persistence/SeedData/categories.json");
                    var categories = JsonConvert.DeserializeObject<List<Category>>(categoryData);

                    if (categories != null)
                    {
                        foreach (var category in categories)
                        {
                            _db.Categories.Add(category);
                        }
                    }

                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Seeded Categories.");
                }
            }).GetAwaiter().GetResult();
        }

        private void AddProducts()
        {
            Task.Run(async () =>
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!_db.Products.Any())
                {
                    var productData = await File.ReadAllTextAsync(path + @"/Persistence/SeedData/products.json");
                    var products = JsonConvert.DeserializeObject<List<Product>>(productData);

                    if (products != null)
                    {
                        foreach (var product in products)
                        {
                            _db.Products.Add(product);
                        }
                    }

                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Seeded Products.");
                }
            }).GetAwaiter().GetResult();
        }
    }
}