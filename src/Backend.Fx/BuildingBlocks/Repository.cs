namespace Backend.Fx.BuildingBlocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Environment.MultiTenancy;
    using Exceptions;
    using Extensions;
    using Logging;
    using Patterns.Authorization;

    public abstract class Repository<TAggregateRoot> : IRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private static readonly ILogger Logger = LogManager.Create<Repository<TAggregateRoot>>();
        private readonly IAggregateAuthorization<TAggregateRoot> aggregateAuthorization;
        private readonly TenantId tenantId;

        protected Repository(TenantId tenantId, IAggregateAuthorization<TAggregateRoot> aggregateAuthorization)
        {
            this.tenantId = tenantId;
            this.aggregateAuthorization = aggregateAuthorization;
        }

        protected static string AggregateTypeName
        {
            get { return typeof(TAggregateRoot).Name; }
        }

        protected abstract IQueryable<TAggregateRoot> RawAggregateQueryable { get; }

        public IQueryable<TAggregateRoot> AggregateQueryable
        {
            get
            {
                if (tenantId.HasValue)
                {
                    return aggregateAuthorization.Filter(RawAggregateQueryable
                            .Where(agg => agg.TenantId == tenantId.Value));
                }

                return RawAggregateQueryable.Where(agg => false);
            }
        }

        public TAggregateRoot Single(int id)
        {
            Logger.Debug($"Removing {AggregateTypeName}[{id}]");
            var aggregateRoot = AggregateQueryable.FirstOrDefault(aggr => aggr.Id.Equals(id));
            if (aggregateRoot == null)
            {
                throw new NotFoundException<TAggregateRoot>(id);
            }

            return aggregateRoot;
        }

        public TAggregateRoot SingleOrDefault(int id)
        {
            return AggregateQueryable.FirstOrDefault(aggr => aggr.Id.Equals(id));
        }

        public TAggregateRoot[] GetAll()
        {
            return AggregateQueryable.ToArray();
        }

        public void Delete(TAggregateRoot aggregateRoot)
        {
            if (aggregateRoot.TenantId != tenantId.Value || !aggregateAuthorization.CanDelete(aggregateRoot))
            {
                throw new System.Security.SecurityException($"You are not allowed to delete {typeof(TAggregateRoot).Name}[{aggregateRoot.Id}]");
            }

            DeletePersistent(aggregateRoot);
        }

        public void Add(TAggregateRoot aggregateRoot)
        {
            if (aggregateAuthorization.CanCreate(aggregateRoot))
            {
                aggregateRoot.TenantId = tenantId.Value;
                AddPersistent(aggregateRoot);
            }
            else
            {
                throw new System.Security.SecurityException($"You are not allowed to create records of type {typeof(TAggregateRoot).Name}");
            }
        }

        public void AddRange(TAggregateRoot[] aggregateRoots)
        {
            aggregateRoots.ForAll(agg =>
            {
                if (!aggregateAuthorization.CanCreate(agg))
                {
                    throw new System.Security.SecurityException($"You are not allowed to create records of type {typeof(TAggregateRoot).Name}");
                }
            });
            aggregateRoots.ForAll(agg => agg.TenantId = tenantId.Value);

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