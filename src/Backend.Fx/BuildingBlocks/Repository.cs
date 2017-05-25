namespace Backend.Fx.BuildingBlocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Environment.MultiTenancy;
    using Exceptions;
    using Patterns.Authorization;
    using Patterns.DependencyInjection;

    public abstract class Repository<TAggregateRoot> : IRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private readonly IAggregateRootAuthorization<TAggregateRoot> aggregateRootAuthorization;
        private readonly ICurrentTHolder<TenantId> tenantIdHolder;

        protected Repository(ICurrentTHolder<TenantId> tenantIdHolder, IAggregateRootAuthorization<TAggregateRoot> aggregateRootAuthorization)
        {
            this.tenantIdHolder = tenantIdHolder;
            this.aggregateRootAuthorization = aggregateRootAuthorization;
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
                var tenantId = tenantIdHolder.Current.Value;
                return RawAggregateQueryable
                        .Where(agg => agg.TenantId == tenantId)
                        .Where(aggregateRootAuthorization.HasAccessExpression);
            }
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

        public void Delete(TAggregateRoot aggregateRoot)
        {
            if (aggregateRoot.TenantId != tenantIdHolder.Current.Value)
            {
                throw new System.Security.SecurityException($"You are not allowed to delete {typeof(TAggregateRoot).Name}[{aggregateRoot.Id}]");
            }
            DeletePersistent(aggregateRoot);
        }

        public void Add(TAggregateRoot aggregateRoot)
        {
            var tenantId = tenantIdHolder.Current.Value;
            if (aggregateRootAuthorization.CanCreate())
            {
                aggregateRoot.TenantId = tenantId;
                AddPersistent(aggregateRoot);
            }
            else
            {
                throw new System.Security.SecurityException($"You are not allowed to create records of type {typeof(TAggregateRoot).Name}");
            }
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

        public TAggregateRoot[] Where(Expression<Func<TAggregateRoot, bool>> predicate)
        {
            return AggregateQueryable.Where(predicate).ToArray();
        }

        protected abstract void AddPersistent(TAggregateRoot aggregateRoot);

        protected abstract void DeletePersistent(TAggregateRoot aggregateRoot);
    }
}