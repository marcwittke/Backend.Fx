using System;
using System.Security.Principal;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Tests.Patterns.UnitOfWork
{
    public class TestUnitOfWork : Fx.Patterns.UnitOfWork.UnitOfWork
    {
        public TestUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder, IDomainEventAggregator eventAggregator, IMessageBusScope messageBusScope)
            : base(clock, identityHolder, eventAggregator, messageBusScope)
        {
        }

        public int RollbackCount { get; private set; }
        public int UpdateTrackingPropertiesCount { get; private set; }
        public int CommitCount { get; private set; }

        protected override void UpdateTrackingProperties(string userId, DateTime utcNow)
        {
            UpdateTrackingPropertiesCount++;
        }

        public override void Complete()
        {
            CommitCount++;
            base.Complete();
        }

        protected override void Dispose(bool disposing)
        {
            RollbackCount++;
            base.Dispose(disposing);
        }
    }
}