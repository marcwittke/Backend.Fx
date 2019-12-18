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
    public class EfUnitOfWork<TDbContext> : UnitOfWork where TDbContext : DbContext
    {
        // private static readonly ILogger Logger = LogManager.Create<EfUnitOfWork<TDbContext>>();
        
        public EfUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder, IDomainEventAggregator eventAggregator,
            IEventBusScope eventBusScope, TDbContext dbContext)
            : base(clock, identityHolder, eventAggregator, eventBusScope)
        {
            DbContext = dbContext;
        }

        public TDbContext DbContext { get; }

        public override void Flush()
        {
            base.Flush();
            DbContext.SaveChanges();
        }

        protected override void UpdateTrackingProperties(string userId, DateTime utcNow)
        {
            DbContext.UpdateTrackingProperties(userId, utcNow);
        }
    }
}
