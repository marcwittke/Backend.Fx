using System.Data;
using System.Security.Principal;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    public class ReadonlyEfUnitOfWork : ReadonlyUnitOfWork
    {
        private readonly EfTransactionManager _transactionManager;

        public ReadonlyEfUnitOfWork(IDbConnection dbConnection, DbContext dbContext, ICurrentTHolder<IIdentity> identityHolder)
            : base(identityHolder)
        {
            dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _transactionManager = new EfTransactionManager(dbConnection, dbContext);
        }

        public override void Begin()
        {
            base.Begin();
            _transactionManager.Begin();
        }
        
        protected override void Rollback()
        {
            _transactionManager.Rollback();
        }
    }
}
