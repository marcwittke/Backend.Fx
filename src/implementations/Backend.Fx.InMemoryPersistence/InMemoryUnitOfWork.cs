using System;
using System.Security.Principal;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.UnitOfWork;

namespace Backend.Fx.InMemoryPersistence
{
    public class InMemoryUnitOfWork : UnitOfWork
    {
        public int CommitCalls { get; private set; }
        public int RollbackCalls { get; private set; }

        public InMemoryUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder, 
                                  IDomainEventAggregator eventAggregator, IEventBusScope eventBusScope) 
                : base(clock, identityHolder, eventAggregator, eventBusScope)
        { }

        protected override void UpdateTrackingProperties(string userId, DateTime utcNow)
        { }

        public override void Complete()
        {
            CommitCalls++;
        }
    }
}
