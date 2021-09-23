using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Exceptions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Backend.Fx.EfCorePersistence
{
    public class EfRepository<TAggregateRoot> : Repository<TAggregateRoot>, IAsyncRepository<TAggregateRoot>
        where TAggregateRoot : AggregateRoot
    {
        private static readonly ILogger Logger = LogManager.Create<EfRepository<TAggregateRoot>>();
        private readonly IAggregateAuthorization<TAggregateRoot> _aggregateAuthorization;
        private readonly IAggregateMapping<TAggregateRoot> _aggregateMapping;

        [SuppressMessage("ReSharper", "EF1001")]
        public EfRepository(
            [JetBrains.Annotations.NotNull] DbContext dbContext,
            IAggregateMapping<TAggregateRoot> aggregateMapping,
            ICurrentTHolder<TenantId> currentTenantIdHolder,
            IAggregateAuthorization<TAggregateRoot> aggregateAuthorization)
            : base(currentTenantIdHolder, aggregateAuthorization)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _aggregateMapping = aggregateMapping;
            _aggregateAuthorization = aggregateAuthorization;

            // somewhat a hack: using the internal EF Core services against advice
            var localViewListener = dbContext.GetService<ILocalViewListener>();
            localViewListener?.RegisterView(AuthorizeChanges);
        }

        public DbContext DbContext { get; }

        protected override IQueryable<TAggregateRoot> RawAggregateQueryable
        {
            get
            {
                IQueryable<TAggregateRoot> queryable = DbContext.Set<TAggregateRoot>();
                if (_aggregateMapping.IncludeDefinitions != null)
                {
                    foreach (Expression<Func<TAggregateRoot, object>> include in _aggregateMapping.IncludeDefinitions)
                    {
                        queryable = queryable.Include(include);
                    }
                }

                return queryable;
            }
        }

        public async Task<TAggregateRoot> SingleAsync(int id, CancellationToken cancellationToken = default)
        {
            return await AggregateQueryable.SingleAsync(agg => agg.Id == id, cancellationToken);
        }

        public async Task<TAggregateRoot> SingleOrDefaultAsync(int id, CancellationToken cancellationToken = default)
        {
            return await AggregateQueryable.SingleOrDefaultAsync(agg => agg.Id == id, cancellationToken);
        }

        public async Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await AggregateQueryable.ToArrayAsync(cancellationToken);
        }

        public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
        {
            return await AggregateQueryable.AnyAsync(cancellationToken);
        }

        public async Task<TAggregateRoot[]> ResolveAsync(
            IEnumerable<int> ids,
            CancellationToken cancellationToken = default)
        {
            if (ids == null)
            {
                return Array.Empty<TAggregateRoot>();
            }

            int[] idsToResolve = ids as int[] ?? ids.ToArray();
            TAggregateRoot[] resolved = await AggregateQueryable.Where(agg => idsToResolve.Contains(agg.Id))
                .ToArrayAsync(cancellationToken);
            if (resolved.Length != idsToResolve.Length)
            {
                throw new ArgumentException(
                    $"The following {AggregateTypeName} ids could not be resolved: {string.Join(", ", idsToResolve.Except(resolved.Select(agg => agg.Id)))}");
            }

            return resolved;
        }

        /// <summary>
        ///     Due to the fact, that a real lifecycle hook API is not yet available (see issue
        /// https://github.com/aspnet/EntityFrameworkCore/issues/626)
        ///     we are using an internal service to achieve the same goal: When a state change occurs from unchanged to modified,
        /// and this repository is
        ///     handling this type of aggregate, we're calling IAggregateAuthorization.CanModify to enforce user privilege
        /// checking.
        ///     We're accepting the possible instability of EF Core internals due to the fact that there is a full API feature in
        /// the pipeline that will
        ///     make this workaround obsolete. Also, not much of an effort was done to write this code, so if we have to deal with
        /// this issue in the future
        ///     again, we do not loose a lot.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="previousState"></param>
        [SuppressMessage("ReSharper", "EF1001")]
        private void AuthorizeChanges(InternalEntityEntry entry, EntityState previousState)
        {
            if (previousState == EntityState.Unchanged && entry.EntityState == EntityState.Modified &&
                entry.EntityType.ClrType == typeof(TAggregateRoot))
            {
                var aggregateRoot = (TAggregateRoot)entry.Entity;
                if (!_aggregateAuthorization.CanModify(aggregateRoot))
                {
                    throw new ForbiddenException(
                            "Unauthorized attempt to modify {AggregateTypeName}[{aggregateRoot.Id}]")
                        .AddError($"You are not allowed to modify {AggregateTypeName}[{aggregateRoot.Id}]");
                }
            }
        }

        protected override void AddPersistent(TAggregateRoot aggregateRoot)
        {
            Logger.Debug($"Persistently adding new {AggregateTypeName}");
            DbContext.Set<TAggregateRoot>().Add(aggregateRoot);
        }

        protected override void AddRangePersistent(TAggregateRoot[] aggregateRoots)
        {
            Logger.Debug($"Persistently adding {aggregateRoots.Length} item(s) of type {AggregateTypeName}");
            DbContext.Set<TAggregateRoot>().AddRange(aggregateRoots);
        }

        protected override void DeletePersistent(TAggregateRoot aggregateRoot)
        {
            Logger.Debug($"Persistently removing {aggregateRoot.DebuggerDisplay}");
            DbContext.Set<TAggregateRoot>().Remove(aggregateRoot);
        }
    }
}
