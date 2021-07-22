using System;
using FluentPOS.Modules.Identity.Core.Entities;
using FluentPOS.Shared.Core.Domain;

namespace FluentPOS.Modules.Identity.Core.Features.Users.Events
{
    public class UserUpdatedEvent : Event
    {
        public Guid Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
        public string UserName { get; }
        public string PhoneNumber { get; }

        public UserUpdatedEvent(FluentUser user)
        {
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            UserName = user.UserName;
            PhoneNumber = user.PhoneNumber;
            Id = user.Id;
            AggregateId = user.Id == Guid.Empty
                ? Guid.NewGuid()
                : user.Id;
        }
    }
}