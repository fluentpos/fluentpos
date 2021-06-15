using FluentPOS.Modules.Catalog.Core.Constants;
using FluentPOS.Shared.Application.Queries;
using FluentPOS.Shared.Application.Wrapper;
using FluentPOS.Shared.DTOs.Catalogs.Categories;
using MediatR;
using System;

namespace FluentPOS.Modules.Catalog.Core.Features.Categories.Queries
{
    public class GetCategoryByIdQuery : IRequest<Result<GetCategoryByIdResponse>>, ICacheable
    {
        public Guid Id { get; set; }
        public bool BypassCache { get; set; }
        public string CacheKey => CatalogCacheKeys.GetCategoryByIdCacheKey(Id);
        public TimeSpan? SlidingExpiration { get; set; }
    }
}