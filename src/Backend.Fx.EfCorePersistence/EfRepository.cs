namespace Backend.Fx.EfCorePersistence
{
    using System.Linq;
    using BuildingBlocks;
    using Environment.MultiTenancy;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Patterns.Authorization;
    using Patterns.DependencyInjection;
    using Patterns.UnitOfWork;

    public class EfRepository<TAggregateRoot> : Repository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private static readonly ILogger Logger = LogManager.Create<EfRepository<TAggregateRoot>>();
        private readonly ICanFlush canFlush;
        private readonly DbContext dbContext;
        private readonly IAggregateRootMapping<TAggregateRoot> aggregateRootMapping;
    
        public EfRepository(ICanFlush canFlush, DbContext dbContext, IAggregateRootMapping<TAggregateRoot> aggregateRootMapping,
            ICurrentTHolder<TenantId> tenantIdHolder, IAggregateRootAuthorization<TAggregateRoot> aggregateRootAuthorization)
            : base(tenantIdHolder, aggregateRootAuthorization)
        {
            this.canFlush = canFlush;
            this.dbContext = dbContext;
            this.aggregateRootMapping = aggregateRootMapping;
        }
        
        protected override void AddPersistent(TAggregateRoot aggregateRoot)
        {
            Logger.Debug($"Persistently adding new {AggregateTypeName}");
            dbContext.Set<TAggregateRoot>().Add(aggregateRoot);
            // to enforce early id generation. We're inside a transaction, therefore it isn't dangerous to call Flush
            canFlush.Flush();
        }

        protected override void DeletePersistent(TAggregateRoot aggregateRoot)
        {
            Logger.Debug($"Persistently removing {aggregateRoot.DebuggerDisplay}");
            dbContext.Set<TAggregateRoot>().Remove(aggregateRoot);
        }

        protected override IQueryable<TAggregateRoot> RawAggregateQueryable
        {
            get
            {
                IQueryable<TAggregateRoot> queryable = dbContext.Set<TAggregateRoot>();
                if (aggregateRootMapping.IncludeDefinitions != null)
                {
                    foreach (var include in aggregateRootMapping.IncludeDefinitions)
                    {
                        queryable = queryable.Include(include);
                    }
                }
                return dbContext.ChangeTracker.AutoDetectChangesEnabled ? queryable : queryable.AsNoTracking();
            }
        }
    }
}
