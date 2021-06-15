using FluentPOS.Modules.Catalog.Core.Constants;
using FluentPOS.Shared.Application.Queries;
using FluentPOS.Shared.Application.Wrapper;
using FluentPOS.Shared.DTOs.Catalogs.Products;
using MediatR;
using System;

namespace FluentPOS.Modules.Catalog.Core.Features.Products.Queries
{
    public class GetProductByIdQuery : IRequest<Result<GetProductByIdResponse>>, ICacheable
    {
        public Guid Id { get; set; }
        public bool BypassCache { get; set; }
        public string CacheKey => CatalogCacheKeys.GetProductByIdCacheKey(Id);
        public TimeSpan? SlidingExpiration { get; set; }
    }
}