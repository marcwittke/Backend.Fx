namespace Backend.Fx.EfCorePersistence
{
    using System.Linq;
    using System.Security;
    using BuildingBlocks;
    using Environment.MultiTenancy;
    using Logging;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Patterns.Authorization;
    using Patterns.DependencyInjection;

    public class EfRepository<TAggregateRoot> : Repository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private static readonly ILogger Logger = LogManager.Create<EfRepository<TAggregateRoot>>();
        private readonly DbContext _dbContext;
        private readonly IAggregateMapping<TAggregateRoot> _aggregateMapping;
        private readonly IAggregateAuthorization<TAggregateRoot> _aggregateAuthorization;

        public EfRepository(DbContext dbContext, IAggregateMapping<TAggregateRoot> aggregateMapping,
            ICurrentTHolder<TenantId> currentTenantIdHolder, IAggregateAuthorization<TAggregateRoot> aggregateAuthorization)
            : base(currentTenantIdHolder, aggregateAuthorization)
        {
            this._dbContext = dbContext;
            this._aggregateMapping = aggregateMapping;
            this._aggregateAuthorization = aggregateAuthorization;

            // somewhat hacky: using the internal EF Core services against advice
            var localViewListener = dbContext.GetService<ILocalViewListener>();
            localViewListener.RegisterView(AuthorizeChanges);
        }
        
        /// <summary>
        /// Due to the fact, that a real lifecycle hook API is not yet available (see issue https://github.com/aspnet/EntityFrameworkCore/issues/626)
        /// we are using an internal service to achieve the same goal: When a state change occurs from unchanged to modified, and this repository is
        /// handling this type of aggregate, we're calling IAggregateAuthorization.CanModify to enforce user privilege checking.
        /// 
        /// We're accepting the possible instability of EF Core internals due to the fact that there is a full API feature in the pipeline that will 
        /// make this workaround obsolete. Also, not much of an effort was done to write this code, so if we have to deal with this issue in the future
        /// again, we do not loose a lot. 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="previousState"></param>
        private void AuthorizeChanges(InternalEntityEntry entry, EntityState previousState)
        {
            if (previousState == EntityState.Unchanged && entry.EntityState == EntityState.Modified && entry.EntityType.ClrType == typeof(TAggregateRoot))
            {
                var aggregateRoot = (TAggregateRoot) entry.Entity;
                if (!_aggregateAuthorization.CanModify(aggregateRoot))
                {
                    throw new SecurityException($"You are not allowed to modify {AggregateTypeName}[{aggregateRoot.Id}]");
                }
            }
        }

        protected override void AddPersistent(TAggregateRoot aggregateRoot)
        {
            Logger.Debug($"Persistently adding new {AggregateTypeName}");
            _dbContext.Set<TAggregateRoot>().Add(aggregateRoot);
        }

        protected override void AddRangePersistent(TAggregateRoot[] aggregateRoots)
        {
            Logger.Debug($"Persistently adding {aggregateRoots.Length} item(s) of type {AggregateTypeName}");
            _dbContext.Set<TAggregateRoot>().AddRange(aggregateRoots);
        }

        protected override void DeletePersistent(TAggregateRoot aggregateRoot)
        {
            Logger.Debug($"Persistently removing {aggregateRoot.DebuggerDisplay}");
            _dbContext.Set<TAggregateRoot>().Remove(aggregateRoot);
        }

        protected override IQueryable<TAggregateRoot> RawAggregateQueryable
        {
            get
            {
                IQueryable<TAggregateRoot> queryable = _dbContext.Set<TAggregateRoot>();
                if (_aggregateMapping.IncludeDefinitions != null)
                {
                    foreach (var include in _aggregateMapping.IncludeDefinitions)
                    {
                        queryable = queryable.Include(include);
                    }
                }
                return queryable;
            }
        }
    }
}
