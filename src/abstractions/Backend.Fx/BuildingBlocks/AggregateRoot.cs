using System.Collections.Generic;
using Backend.Fx.Patterns.EventAggregation.Domain;

namespace Backend.Fx.BuildingBlocks
{
    /// <summary>
    ///     A collection of objects that are bound together by a root entity
    ///     See https://en.wikipedia.org/wiki/Domain-driven_design#Building_blocks
    /// </summary>
    public abstract class AggregateRoot : Entity
    {
        protected AggregateRoot()
        {
        }

        protected AggregateRoot(int id) : base(id)
        {
        }

        public int TenantId { get; set; }

        internal ISet<IDomainEvent> DomainEvents { get; } = new HashSet<IDomainEvent>();

        protected void PublishDomainEvent(IDomainEvent domainEvent)
        {
            DomainEvents.Add(domainEvent);
        }
    }
}