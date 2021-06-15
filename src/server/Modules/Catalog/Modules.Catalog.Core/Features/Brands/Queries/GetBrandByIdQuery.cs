using FluentPOS.Modules.Catalog.Core.Constants;
using FluentPOS.Shared.Application.Queries;
using FluentPOS.Shared.Application.Wrapper;
using FluentPOS.Shared.DTOs.Catalogs.Brands;
using MediatR;
using System;

namespace FluentPOS.Modules.Catalog.Core.Features.Brands.Queries
{
    public class GetBrandByIdQuery : IRequest<Result<GetBrandByIdResponse>>, ICacheable
    {
        public Guid Id { get; set; }
        public bool BypassCache { get; set; }
        public string CacheKey => CatalogCacheKeys.GetBrandByIdCacheKey(Id);
        public TimeSpan? SlidingExpiration { get; set; }
    }
}