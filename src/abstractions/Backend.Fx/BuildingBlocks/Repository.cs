using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Exceptions;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DependencyInjection;
using JetBrains.Annotations;

namespace Backend.Fx.BuildingBlocks
{
    public abstract class Repository<TAggregateRoot> : IRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private static readonly ILogger Logger = LogManager.Create<Repository<TAggregateRoot>>();
        private readonly IAggregateAuthorization<TAggregateRoot> _aggregateAuthorization;
        private readonly ICurrentTHolder<TenantId> _tenantIdHolder;

        protected Repository(ICurrentTHolder<TenantId> tenantIdHolder, IAggregateAuthorization<TAggregateRoot> aggregateAuthorization)
        {
            Logger.Trace(
                $"Instantiating a new Repository<{AggregateTypeName}> for tenant [{(tenantIdHolder.Current.HasValue ? tenantIdHolder.Current.Value.ToString() : "null")}]");
            _tenantIdHolder = tenantIdHolder;
            _aggregateAuthorization = aggregateAuthorization;
        }

        protected static string AggregateTypeName => typeof(TAggregateRoot).Name;

        public TAggregateRoot Single(int id)
        {
            Logger.Debug($"Getting single {AggregateTypeName}[{id}]");
            return Find(id) ?? throw new NotFoundException<TAggregateRoot>(id);
        }

        public TAggregateRoot SingleOrDefault(int id)
        {
            Logger.Debug($"Getting single or default {AggregateTypeName}[{id}]");
            return Find(id);
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
            return AnyPersistent(_tenantIdHolder.Current, _aggregateAuthorization);
        }

        public TAggregateRoot[] Resolve(IEnumerable<int> ids)
        {
            if (ids == null)
            {
                return Array.Empty<TAggregateRoot>();
            }

            int[] idsToResolve = ids as int[] ?? ids.ToArray();

            if (idsToResolve.Length == 0)
            {
                return Array.Empty<TAggregateRoot>();
            }

            IQueryable<TAggregateRoot> queryable = FindManyPersistent(idsToResolve, _tenantIdHolder.Current, _aggregateAuthorization).AsQueryable();
            
            TAggregateRoot[] resolved = queryable.ToArray();
            if (resolved.Length != idsToResolve.Length)
            {
                throw new ArgumentException($"The following {AggregateTypeName} ids could not be resolved: " +
                                            $"{string.Join(", ", idsToResolve.Except(resolved.Select(agg => agg.Id)))}");
            }

            return resolved;
        }

        private TAggregateRoot Find(int id)
        {
            return FindPersistent(id, _tenantIdHolder.Current, _aggregateAuthorization);
        }
        
        protected abstract TAggregateRoot FindPersistent(int id, TenantId tenantId, IAggregateAuthorization<TAggregateRoot> authorization);
        protected abstract TAggregateRoot[] FindManyPersistent(int[] ids, TenantId tenantId, IAggregateAuthorization<TAggregateRoot> authorization);
        protected abstract bool AnyPersistent(TenantId tenantId, IAggregateAuthorization<TAggregateRoot> authorization);

        protected abstract void AddPersistent(TAggregateRoot aggregateRoot);

        protected abstract void AddRangePersistent(TAggregateRoot[] aggregateRoots);

        protected abstract void DeletePersistent(TAggregateRoot aggregateRoot);
    }
}