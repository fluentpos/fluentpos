using FluentPOS.Shared.DTOs.Filters;

namespace FluentPOS.Shared.DTOs.Catalogs.Brands
{
    public class PaginatedBrandFilter : PaginatedFilter
    {
        public string SearchString { get; set; }
    }
}