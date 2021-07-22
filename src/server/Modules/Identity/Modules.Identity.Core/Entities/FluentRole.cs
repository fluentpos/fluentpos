using FluentPOS.Shared.Core.Contracts;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using FluentPOS.Modules.Identity.Core.Entities.ExtendedAttributes;
using FluentPOS.Shared.Core.Domain;
using System;

namespace FluentPOS.Modules.Identity.Core.Entities
{
    public class FluentRole : IdentityRole<Guid>, IEntity<Guid>, IBaseEntity
    {
        public string Description { get; set; }
        public virtual ICollection<FluentRoleClaim> RoleClaims { get; set; }
        public virtual ICollection<RoleExtendedAttribute> ExtendedAttributes { get; set; }

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

        public FluentRole() : base()
        {
            RoleClaims = new HashSet<FluentRoleClaim>();
            ExtendedAttributes = new HashSet<RoleExtendedAttribute>();
        }

        public FluentRole(string roleName, string roleDescription = null) : base(roleName)
        {
            RoleClaims = new HashSet<FluentRoleClaim>();
            ExtendedAttributes = new HashSet<RoleExtendedAttribute>();
            Description = roleDescription;
        }
    }
}