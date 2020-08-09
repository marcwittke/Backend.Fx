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
        public InMemoryUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder,
                                  IDomainEventAggregator eventAggregator, IMessageBusScope messageBusScope)
            : base(clock, identityHolder, eventAggregator, messageBusScope)
        {
        }

        public int CommitCalls { get; private set; }

        protected override void UpdateTrackingProperties(string userId, DateTime utcNow)
        {
        }

        public override void Complete()
        {
            CommitCalls++;
        }
    }
}