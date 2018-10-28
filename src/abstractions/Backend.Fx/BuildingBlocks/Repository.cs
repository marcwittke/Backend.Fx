namespace Backend.Fx.BuildingBlocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Environment.MultiTenancy;
    using Exceptions;
    using Extensions;
    using JetBrains.Annotations;
    using Logging;
    using Patterns.Authorization;
    using Patterns.DependencyInjection;

    public abstract class Repository<TAggregateRoot> : IRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private static readonly ILogger Logger = LogManager.Create<Repository<TAggregateRoot>>();
        private readonly IAggregateAuthorization<TAggregateRoot> _aggregateAuthorization;
        private readonly ICurrentTHolder<TenantId> _tenantIdHolder;

        protected Repository(ICurrentTHolder<TenantId> tenantIdHolder, IAggregateAuthorization<TAggregateRoot> aggregateAuthorization)
        {
            Logger.Trace($"Instantiating a new Repository<{AggregateTypeName}> for tenant [{(tenantIdHolder.Current.HasValue ? tenantIdHolder.Current.Value.ToString() : "null")}]");
            _tenantIdHolder = tenantIdHolder;
            _aggregateAuthorization = aggregateAuthorization;
        }

        protected static string AggregateTypeName => typeof(TAggregateRoot).Name;

        protected abstract IQueryable<TAggregateRoot> RawAggregateQueryable { get; }

        public IQueryable<TAggregateRoot> AggregateQueryable
        {
            get
            {
                if (_tenantIdHolder.Current.HasValue)
                {
                    return _aggregateAuthorization.Filter(RawAggregateQueryable
                            .Where(agg => agg.TenantId == _tenantIdHolder.Current.Value));
                }

                return RawAggregateQueryable.Where(agg => false);
            }
        }

        public TAggregateRoot Single(int id)
        {
            Logger.Debug($"Getting single {AggregateTypeName}[{id}]");
            var aggregateRoot = AggregateQueryable.FirstOrDefault(aggr => aggr.Id.Equals(id));
            if (aggregateRoot == null)
            {
                throw new NotFoundException<TAggregateRoot>(id);
            }

            return aggregateRoot;
        }

        public TAggregateRoot SingleOrDefault(int id)
        {
            Logger.Debug($"Getting single or default {AggregateTypeName}[{id}]");
            return AggregateQueryable.FirstOrDefault(aggr => aggr.Id.Equals(id));
        }

        public TAggregateRoot[] GetAll()
        {
            return AggregateQueryable.ToArray();
        }

        public void Delete([NotNull] TAggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException(nameof(aggregateRoot));
            }

            Logger.Debug($"Deleting {AggregateTypeName}[{aggregateRoot.Id}]");
            if (aggregateRoot.TenantId != _tenantIdHolder.Current.Value || !_aggregateAuthorization.CanDelete(aggregateRoot))
            {
                throw new ForbiddenException($"You are not allowed to delete {typeof(TAggregateRoot).Name}[{aggregateRoot.Id}]");
            }

            DeletePersistent(aggregateRoot);
        }

        public void Add([NotNull] TAggregateRoot aggregateRoot)
        {
            if (aggregateRoot == null)
            {
                throw new ArgumentNullException(nameof(aggregateRoot));
            }

            if (_aggregateAuthorization.CanCreate(aggregateRoot))
            {
                Logger.Debug($"Adding {AggregateTypeName}[{aggregateRoot.Id}]");
                aggregateRoot.TenantId = _tenantIdHolder.Current.Value;
                AddPersistent(aggregateRoot);
            }
            else
            {
                throw new ForbiddenException($"You are not allowed to create records of type {typeof(TAggregateRoot).Name}");
            }
        }

        public void AddRange([NotNull] TAggregateRoot[] aggregateRoots)
        {
            if (aggregateRoots == null)
            {
                throw new ArgumentNullException(nameof(aggregateRoots));
            }

            aggregateRoots.ForAll(agg =>
            {
                if (!_aggregateAuthorization.CanCreate(agg))
                {
                    throw new ForbiddenException($"You are not allowed to create records of type {typeof(TAggregateRoot).Name}");
                }
            });
            
            Logger.Debug($"Adding {aggregateRoots.Length} items of type {AggregateTypeName}");

            aggregateRoots.ForAll(agg => agg.TenantId = _tenantIdHolder.Current.Value);

            AddRangePersistent(aggregateRoots);
        }

        public bool Any()
        {
            return AggregateQueryable.Any();
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

        protected abstract void AddPersistent(TAggregateRoot aggregateRoot);

        protected abstract void AddRangePersistent(TAggregateRoot[] aggregateRoots);

        protected abstract void DeletePersistent(TAggregateRoot aggregateRoot);
    }
}