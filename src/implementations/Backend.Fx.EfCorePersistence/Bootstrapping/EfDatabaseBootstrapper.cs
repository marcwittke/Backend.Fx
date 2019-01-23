using System.Collections.Generic;
using System.Data;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence.Bootstrapping
{
    public abstract class EfDatabaseBootstrapper<TDbContext> : IDatabaseBootstrapper where TDbContext : DbContext
    {
        private static readonly ILogger Logger = LogManager.Create<EfDatabaseBootstrapper<TDbContext>>();
        private readonly IScopeManager _scopeManager;

        protected EfDatabaseBootstrapper(IScopeManager scopeManager)
        {
            _scopeManager = scopeManager;
        }

        public void EnsureDatabaseExistence()
        {
            Logger.Info("Ensuring database existence");
            using (var scope = _scopeManager.BeginScope(new SystemIdentity(), new TenantId(null)))
            {
                using (var dbContext = scope.GetInstance<TDbContext>())
                {
                    ExecuteCreationStrategy(dbContext);
                    EnsureSearchIndexExistence(dbContext, scope.GetAllInstances<IFullTextSearchIndex>());
                    EnsureSequenceExistence(scope.GetInstance<IDbConnection>(), scope.GetAllInstances<ISequence>());
                }
            }
        }

        
        private void EnsureSearchIndexExistence(TDbContext dbContext, IEnumerable<IFullTextSearchIndex> fullTextSearchIndices)
        {
            Logger.Info("Ensuring existence of full text search indizes");
            foreach (var fullTextSearchIndex in fullTextSearchIndices)
            {
                fullTextSearchIndex.EnsureIndex(dbContext);
            }
        }

        private void EnsureSequenceExistence(IDbConnection dbConnection, IEnumerable<ISequence> sequences)
        {
            Logger.Info("Ensuring existence of sequences");
            foreach (var sequence in sequences)
            {
                sequence.EnsureSequence(dbConnection);
            }
        }

        protected abstract void ExecuteCreationStrategy(DbContext dbContext);
    }
}
