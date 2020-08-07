﻿namespace Backend.Fx.Tests.Patterns.UnitOfWork
{
    using System;
    using System.Security.Principal;
    using Fx.Environment.DateAndTime;
    using Fx.Patterns.DependencyInjection;
    using Fx.Patterns.EventAggregation.Domain;
    using Fx.Patterns.EventAggregation.Integration;
    using Fx.Patterns.UnitOfWork;

    public class TestUnitOfWork : UnitOfWork
    {
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

        public TestUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder, IDomainEventAggregator eventAggregator, IMessageBusScope messageBusScope) 
                : base(clock, identityHolder, eventAggregator, messageBusScope)
        { }
    }
}