using System;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Identity.Core.Features.Roles.Events
{
    public class RoleDeletedEvent : Event
    {
        public Guid Id { get; }

        public RoleDeletedEvent(Guid id)
        {
            Id = id;
            AggregateId = id == Guid.Empty
                ? Guid.NewGuid()
                : id;
        }
    }
}