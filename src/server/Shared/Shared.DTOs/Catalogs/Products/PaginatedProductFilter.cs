using FluentPOS.Shared.DTOs.Filters;

namespace FluentPOS.Shared.DTOs.Catalogs.Products
{
    public class PaginatedProductFilter : PaginatedFilter
    {
        public string SearchString { get; set; }
    }
}