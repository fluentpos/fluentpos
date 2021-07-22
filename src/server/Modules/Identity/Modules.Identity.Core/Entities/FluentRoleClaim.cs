using System;
using System.Collections.Generic;
using FluentPOS.Shared.Core.Contracts;
using FluentPOS.Shared.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace FluentPOS.Modules.Identity.Core.Entities
{
    public class FluentRoleClaim : IdentityRoleClaim<Guid>, IBaseEntity
    {
        public new Guid Id { get; set; }
        public string Description { get; set; }
        public string Group { get; set; }
        public virtual FluentRole Role { get; set; }

        private List<Event> _domainEvents;
        public IReadOnlyCollection<Event> DomainEvents => _domainEvents?.AsReadOnly();

        public void AddDomainEvent(Event domainEvent)
        {
            _domainEvents ??= new List<Event>();
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(Event domainEvent)
        {
            _domainEvents?.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }

        public FluentRoleClaim() : base()
        {
        }

        public FluentRoleClaim(string roleClaimDescription = null, string roleClaimGroup = null) : base()
        {
            Description = roleClaimDescription;
            Group = roleClaimGroup;
        }
    }
}