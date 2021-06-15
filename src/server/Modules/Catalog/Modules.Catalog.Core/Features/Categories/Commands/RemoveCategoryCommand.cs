using FluentPOS.Shared.Application.Wrapper;
using MediatR;
using System;

namespace FluentPOS.Modules.Catalog.Core.Features.Categories.Commands
{
    public class RemoveCategoryCommand : IRequest<Result<Guid>>
    {
        public Guid Id { get; set; }
    }
}