using FluentPOS.Shared.Application.Wrapper;
using MediatR;
using System;

namespace FluentPOS.Modules.Catalog.Core.Features.Products.Queries
{
    public class GetProductImageQuery : IRequest<Result<string>>
    {
        public Guid Id { get; set; }

        public GetProductImageQuery(Guid productId)
        {
            Id = productId;
        }
    }
}