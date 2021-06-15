using FluentPOS.Shared.Application.Wrapper;
using MediatR;
using System;

namespace FluentPOS.Modules.Catalog.Core.Features.Brands.Queries
{
    public class GetBrandImageQuery : IRequest<Result<string>>
    {
        public Guid Id { get; set; }

        public GetBrandImageQuery(Guid brandId)
        {
            Id = brandId;
        }
    }
}