using System;

namespace FluentPOS.Modules.Catalog.Core.Constants
{
    /// <summary>
    /// Cache Keys for Catalog Module
    /// </summary>
    public static class CatalogCacheKeys
    {
        /// <summary>
        /// Generates Cache Keys for Brand Id
        /// </summary>
        /// <param name="id">Guid</param>
        /// <returns></returns>
        public static string GetBrandByIdCacheKey(Guid id)
        {
            return $"Brand-{id}";
        }

        public static string GetCategoryByIdCacheKey(Guid id)
        {
            return $"Category-{id}";
        }

        public static string GetProductByIdCacheKey(Guid id)
        {
            return $"Product-{id}";
        }
    }
}