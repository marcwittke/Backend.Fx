namespace Backend.Fx.Tests.Patterns.UnitOfWork
{
    using System;
    using System.Security.Principal;
    using Fx.Environment.DateAndTime;
    using Fx.Patterns.DependencyInjection;
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

        protected override void Commit()
        {
            CommitCount++;
        }

        protected override void Rollback()
        {
            RollbackCount++;
        }

        public TestUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder) : base(clock, identityHolder)
        { }
    }
}