﻿namespace Backend.Fx.EfCorePersistence
{
    using System.Linq;
    using BuildingBlocks;
    using Environment.MultiTenancy;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Patterns.Authorization;
    using Patterns.UnitOfWork;

    public class EfRepository<TAggregateRoot> : Repository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private static readonly ILogger Logger = LogManager.Create<EfRepository<TAggregateRoot>>();
        private readonly ICanFlush canFlush;
        private readonly DbContext dbContext;
        private readonly IAggregateRootMapping<TAggregateRoot> aggregateRootMapping;
    
        public EfRepository(ICanFlush canFlush, DbContext dbContext, IAggregateRootMapping<TAggregateRoot> aggregateRootMapping,
            TenantId tenantId, IAggregateAuthorization<TAggregateRoot> aggregateAuthorization)
            : base(tenantId, aggregateAuthorization)
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

        protected override void AddRangePersistent(TAggregateRoot[] aggregateRoots)
        {
            Logger.Debug($"Persistently adding {aggregateRoots.Length} item(s) of type {AggregateTypeName}");
            dbContext.Set<TAggregateRoot>().AddRange(aggregateRoots);
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
