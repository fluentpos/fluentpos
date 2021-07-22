using System;
using FluentPOS.Modules.Identity.Core.Entities;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Identity.Core.Features.Roles.Events
{
    public class RoleAddedEvent : Event
    {
        public Guid Id { get; }
        public string Name { get; }
        public string Description { get; }

        public RoleAddedEvent(FluentRole role)
        {
            Name = role.Name;
            Description = role.Description;
            Id = role.Id;
            AggregateId = role.Id == Guid.Empty
                ? Guid.NewGuid()
                : role.Id;
        }
    }
}