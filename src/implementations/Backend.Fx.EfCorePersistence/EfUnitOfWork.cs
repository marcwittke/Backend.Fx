using System;
using System.Data.Common;
using System.Security.Principal;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    public class EfUnitOfWork<TDbContext> : DbUnitOfWork where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<EfUnitOfWork<TDbContext>>();
        
        private readonly Func<DbConnection, TDbContext> _dbContextFactory;

        public EfUnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder, IDomainEventAggregator eventAggregator,
            IEventBusScope eventBusScope, Func<DbConnection, TDbContext> dbContextFactory, DbConnection dbConnection)
            : base(clock, identityHolder, eventAggregator, eventBusScope, dbConnection)
        {
            _dbContextFactory = dbContextFactory;
        }

        public TDbContext DbContext { get; private set; }

        public override void Begin()
        {
            base.Begin();
            
            DbContext = _dbContextFactory.Invoke(Connection);
            DbContext.Database.UseTransaction(CurrentTransaction);
        }

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
