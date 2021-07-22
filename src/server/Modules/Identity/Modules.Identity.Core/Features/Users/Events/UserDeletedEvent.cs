using System;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Identity.Core.Features.Users.Events
{
    public class UserDeletedEvent : Event
    {
        public Guid Id { get; }

        public UserDeletedEvent(Guid id)
        {
            Id = id;
            AggregateId = id == Guid.Empty
                ? Guid.NewGuid()
                : id;
        }
    }
}