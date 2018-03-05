namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System;
    using System.Security.Principal;
    using Environment.DateAndTime;
    using Patterns.DependencyInjection;
    using Patterns.UnitOfWork;

    public class InMemoryUnitOfWork : UnitOfWork
    {
        public int CommitCalls { get; private set; }
        public int RollbackCalls { get; private set; }

        public InMemoryUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder) : base(clock, identityHolder)
        { }

        protected override void UpdateTrackingProperties(string userId, DateTime utcNow)
        { }

        protected override void Commit()
        {
            CommitCalls++;
        }

        protected override void Rollback()
        {
            RollbackCalls++;
        }
    }
}
