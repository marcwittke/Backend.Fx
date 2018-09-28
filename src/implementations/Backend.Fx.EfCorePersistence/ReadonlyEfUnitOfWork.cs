namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Security.Principal;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Patterns.DependencyInjection;
    using Patterns.UnitOfWork;

    public class ReadonlyEfUnitOfWork : ReadonlyUnitOfWork
    {
        private static readonly ILogger Logger = LogManager.Create<EfUnitOfWork>();
        private readonly DbContext _dbContext;
        private IDisposable _transactionLifetimeLogger;
        private IDbContextTransaction _currentTransaction;

        public ReadonlyEfUnitOfWork(DbContext dbContext, ICurrentTHolder<IIdentity> identityHolder) : base(identityHolder)
        {
            this._dbContext = dbContext;
            this._dbContext.ChangeTracker.AutoDetectChangesEnabled = false;
            this._dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public override void Begin()
        {
            base.Begin();
            BeginTransaction();
        }
        
        protected override void Rollback()
        {
            Logger.Info("Rolling back transaction");
            try
            {
                _currentTransaction?.Rollback();
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Rollback failed");
            }
            _transactionLifetimeLogger?.Dispose();
            _transactionLifetimeLogger = null;
        }

        private void BeginTransaction()
        {
            _currentTransaction = _dbContext.Database.BeginTransaction();
            _transactionLifetimeLogger = Logger.InfoDuration("Transaction open");
        }
    }
}
