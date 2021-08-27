using System.Linq;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DependencyInjection;

namespace Backend.Fx.BuildingBlocks
{
    public abstract class QueryableRepository<TAggregateRoot> : Repository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private static readonly ILogger Logger = LogManager.Create<Repository<TAggregateRoot>>();

        protected QueryableRepository(ICurrentTHolder<TenantId> tenantIdHolder,
                                      IAggregateAuthorization<TAggregateRoot> aggregateAuthorization)
            : base(tenantIdHolder, aggregateAuthorization)
        {
            Logger.Trace(
                $"Instantiating a new Repository<{AggregateTypeName}> for tenant [{(tenantIdHolder.Current.HasValue ? tenantIdHolder.Current.Value.ToString() : "null")}]");
        }

        protected abstract IQueryable<TAggregateRoot> RawAggregateQueryable { get; }

        protected IQueryable<TAggregateRoot> GetAggregateQueryable(TenantId tenantId, IAggregateAuthorization<TAggregateRoot> auth)
        {
            if (tenantId.HasValue)
            {
                var queryable = RawAggregateQueryable.Where(agg => agg.TenantId == tenantId.Value);
                queryable = RawAggregateQueryable.Where(auth.HasAccessExpression);
                queryable = auth.Filter(queryable);
                return queryable;
            }

            return RawAggregateQueryable.Where(agg => false);
        }


        protected override TAggregateRoot FindPersistent(int id, TenantId tenantId, IAggregateAuthorization<TAggregateRoot> authorization)
        {
            var q = GetAggregateQueryable(tenantId, authorization);
            return q.FirstOrDefault(agg => agg.Id.Equals(id));
        }

        protected override TAggregateRoot[] FindManyPersistent(int[] ids, TenantId tenantId, IAggregateAuthorization<TAggregateRoot> authorization)
        {
            var q = GetAggregateQueryable(tenantId, authorization);
            return q.Where(agg => ids.Contains(agg.Id)).ToArray();
        }

        protected override bool AnyPersistent(TenantId tenantId, IAggregateAuthorization<TAggregateRoot> authorization)
        {
            var q = GetAggregateQueryable(tenantId, authorization);
            return q.Any();
        }
    }
}