using System.Security.Principal;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Backend.Fx.EfCorePersistence
{
    public class EfUnitOfWork : UnitOfWork
    {
        public EfUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder, IDomainEventAggregator eventAggregator,
            IEventBusScope eventBusScope, DbContext dbContext)
            : base(clock, identityHolder, eventAggregator, eventBusScope)
        {
            DbContext = dbContext;
            DbContext.ChangeTracker.StateChanged += UpdateTrackingPropertiesOnStateChange;
        }

        public DbContext DbContext { get; }

        public override void Flush()
        {
            base.Flush();
            DbContext.SaveChanges();
        }

        
        private void UpdateTrackingPropertiesOnStateChange(object sender, EntityStateChangedEventArgs e)
        {
            e.Entry.UpdateTrackingProperties(IdentityHolder, Clock, DbContext.ChangeTracker, e.NewState);
        }

        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DbContext != null)
                {
                    DbContext.ChangeTracker.StateChanged -= UpdateTrackingPropertiesOnStateChange;
                    DbContext.Dispose();    
                }
                
            }
            base.Dispose(disposing);
        }
    }
}
