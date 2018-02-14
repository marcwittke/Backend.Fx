namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System;
    using System.Security.Principal;
    using Environment.DateAndTime;
    using Patterns.UnitOfWork;

    public class InMemoryUnitOfWork : UnitOfWork
    {
        public int CommitCalls { get; private set; }
        public int RollbackCalls { get; private set; }

        public InMemoryUnitOfWork(IClock clock, IIdentity identity) : base(clock, identity)
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
