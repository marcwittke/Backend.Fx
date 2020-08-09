using System;
using System.Security.Principal;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    public class EfUnitOfWork : UnitOfWork
    {
        public EfUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder, IDomainEventAggregator eventAggregator,
                            IMessageBusScope messageBusScope, DbContext dbContext)
            : base(clock, identityHolder, eventAggregator, messageBusScope)
        {
            DbContext = dbContext;
            DbContext.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public DbContext DbContext { get; }

        public override void Flush()
        {
            DbContext.ChangeTracker.DetectChanges();
            base.Flush();
            DbContext.SaveChanges();
        }

        protected override void UpdateTrackingProperties(string userId, DateTime utcNow)
        {
            DbContext.UpdateTrackingProperties(userId, utcNow);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) DbContext?.Dispose();
            base.Dispose(disposing);
        }
    }
}