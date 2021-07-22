using System;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Identity.Core.Features.RoleClaims.Events
{
    public class RoleClaimDeletedEvent : Event
    {
        public Guid Id { get; }

        public RoleClaimDeletedEvent(Guid id)
        {
            Id = id;
            AggregateId = Guid.NewGuid();
        }
    }
}