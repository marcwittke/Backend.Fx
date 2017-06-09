namespace Backend.Fx.Testing.InMemoryPersistence
{
    using System;
    using System.Security.Principal;
    using Environment.DateAndTime;
    using Patterns.UnitOfWork;

    public class InMemoryUnitOfWork : UnitOfWork
    {
        public InMemoryUnitOfWork(IClock clock, IIdentity identity) : base(clock, identity)
        { }

        protected override void UpdateTrackingProperties(string userId, DateTime utcNow)
        { }

        protected override void Commit()
        { }

        protected override void Rollback()
        { }
    }
}
