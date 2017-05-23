namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security;
    using BuildingBlocks;
    using Environment.MultiTenancy;
    using Exceptions;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Patterns.DependencyInjection;
    using Patterns.UnitOfWork;

    public class EfRepository<TAggregateRoot> : IRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private static readonly ILogger Logger = LogManager.Create<EfRepository<TAggregateRoot>>();

        private readonly ICanFlush canFlush;
        private readonly DbContext dbContext;
        private readonly IAggregateRootMapping<TAggregateRoot> aggregateRootMapping;
        private readonly ICurrentTHolder<TenantId> tenantIdHolder;

        public EfRepository(ICanFlush canFlush, DbContext dbContext, IAggregateRootMapping<TAggregateRoot> aggregateRootMapping, ICurrentTHolder<TenantId> tenantIdHolder)
        {
            this.canFlush = canFlush;
            this.dbContext = dbContext;
            this.aggregateRootMapping = aggregateRootMapping;
            this.tenantIdHolder = tenantIdHolder;
        }

        public TAggregateRoot Single(int id)
        {
            var aggregateRoot = AggregateQueryable.SingleOrDefault(aggr => aggr.Id.Equals(id));
            if (aggregateRoot == null)
            {
                throw new NotFoundException<TAggregateRoot>(id);
            }

            return aggregateRoot;
        }

        public TAggregateRoot SingleOrDefault(int id)
        {
            return AggregateQueryable.SingleOrDefault(aggr => aggr.Id.Equals(id));
        }

        public TAggregateRoot[] GetAll()
        {
            return AggregateQueryable.ToArray();
        }

        public void Delete(int id)
        {
            using (Logger.DebugDuration(string.Format("Removing {0}[{1}]", AggregateTypeName, id)))
            {
                var entry = dbContext.ChangeTracker.Entries<TAggregateRoot>().FirstOrDefault(entr => entr.Entity.Id == id);
                if (entry == null)
                {
                    // we're deleting an aggregate root, that has not been loaded before. 
                    Delete(Single(id));
                }
                else
                {
                    if (entry.Entity.TenantId != tenantIdHolder.Current.Value)
                    {
                        throw new SecurityException($"You are not allowed to delete {typeof(TAggregateRoot).Name}[{entry.Entity.Id}]");
                    }
                    entry.State = EntityState.Deleted;
                }
            }
        }

        public void Delete(TAggregateRoot aggregateRoot)
        {
            if (aggregateRoot.TenantId != tenantIdHolder.Current.Value)
            {
                throw new SecurityException($"You are not allowed to delete {typeof(TAggregateRoot).Name}[{aggregateRoot.Id}]");
            }
            dbContext.Set<TAggregateRoot>().Remove(aggregateRoot);
        }

        public void Add(TAggregateRoot aggregateRoot)
        {
            aggregateRoot.TenantId = tenantIdHolder.Current.Value;
            dbContext.Set<TAggregateRoot>().Add(aggregateRoot);
            // to enforce early id generation. We're inside a transaction, therefore it isn't dangerous to call Flush
            canFlush.Flush();
        }

        public bool Any()
        {
            return dbContext.Set<TAggregateRoot>().Any(agg => agg.TenantId == tenantIdHolder.Current.Value);
        }

        public TAggregateRoot[] Where(Expression<Func<TAggregateRoot, bool>> predicate)
        {
            return AggregateQueryable.Where(predicate).ToArray();
        }
        
        public TAggregateRoot[] Resolve(IEnumerable<int> ids)
        {
            if (ids == null)
            {
                return new TAggregateRoot[0];
            }

            int[] idsToResolve = ids as int[] ?? ids.ToArray();
            TAggregateRoot[] resolved = AggregateQueryable.Where(agg => idsToResolve.Contains(agg.Id)).ToArray();
            if (resolved.Length != idsToResolve.Length)
            {
                throw new ArgumentException($"The following {AggregateTypeName} ids could not be resolved: {string.Join(", ", idsToResolve.Except(resolved.Select(agg => agg.Id)))}");
            }
            return resolved;
        }
        
        public IQueryable<TAggregateRoot> AggregateQueryable
        {
            get
            {
                IQueryable<TAggregateRoot> queryable = dbContext.Set<TAggregateRoot>().Where(agg => agg.TenantId == tenantIdHolder.Current.Value);
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

        protected static string AggregateTypeName
        {
            get { return typeof(TAggregateRoot).Name; }
        }
    }
}
