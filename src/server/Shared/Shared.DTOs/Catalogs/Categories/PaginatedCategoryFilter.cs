using FluentPOS.Shared.DTOs.Filters;

namespace FluentPOS.Shared.DTOs.Catalogs.Categories
{
    public class PaginatedCategoryFilter : PaginatedFilter
    {
        public string SearchString { get; set; }
    }
}