namespace FluentPOS.Shared.DTOs.Filters
{
    public class PaginatedFilter : BaseFilter
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public PaginatedFilter()
        {
            this.PageNumber = 1;
            this.PageSize = 10;
        }

        public PaginatedFilter(int pageNumber, int pageSize)
        {
            this.PageNumber = pageNumber < 1 ? 1 : pageNumber;
            this.PageSize = pageSize > 10 ? 10 : pageSize;
        }
    }
}