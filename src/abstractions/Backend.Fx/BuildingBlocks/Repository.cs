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
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.BuildingBlocks
{
    public abstract class Repository<TAggregateRoot> : IRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private static readonly ILogger Logger = Log.Create<Repository<TAggregateRoot>>();
        private readonly IAggregateAuthorization<TAggregateRoot> _aggregateAuthorization;
        private readonly ICurrentTHolder<TenantId> _tenantIdHolder;

        protected Repository(ICurrentTHolder<TenantId> tenantIdHolder, IAggregateAuthorization<TAggregateRoot> aggregateAuthorization)
        {
            Logger.LogTrace(
                "Instantiating a new Repository<{AggregateTypeName}> for tenant [{TenantId}]",
                    AggregateTypeName,
                    tenantIdHolder.Current.HasValue ? tenantIdHolder.Current.Value.ToString() : "null");
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
            Logger.LogDebug("Getting single {AggregateTypeName}[{Id}]", AggregateTypeName, id);
            TAggregateRoot aggregateRoot = AggregateQueryable.FirstOrDefault(agg => agg.Id.Equals(id));
            if (aggregateRoot == null)
            {
                throw new NotFoundException<TAggregateRoot>(id);
            }

            return aggregateRoot;
        }

        public TAggregateRoot SingleOrDefault(int id)
        {
            Logger.LogDebug("Getting single or default {AggregateTypeName}[{Id}]", AggregateTypeName, id);
            return AggregateQueryable.FirstOrDefault(agg => agg.Id.Equals(id));
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

            Logger.LogDebug("Deleting {AggregateTypeName}[{Id}]", AggregateTypeName, aggregateRoot.Id);
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
                Logger.LogDebug("Adding {AggregateTypeName}[{Id}]", AggregateTypeName, aggregateRoot.Id);
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
            
            Logger.LogDebug("Adding {Count} items of type {AggregateTypeName}", aggregateRoots.Length, AggregateTypeName);

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
                return Array.Empty<TAggregateRoot>();
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