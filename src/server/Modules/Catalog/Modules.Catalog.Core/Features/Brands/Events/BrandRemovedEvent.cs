using FluentPOS.Shared.Application.Domain;
using System;

namespace FluentPOS.Modules.Catalog.Core.Features.Brands.Events
{
    public class BrandRemovedEvent : Event
    {
        public Guid Id { get; set; }

        public BrandRemovedEvent(Guid id)
        {
            Id = id;
            AggregateId = id;
        }
    }
}