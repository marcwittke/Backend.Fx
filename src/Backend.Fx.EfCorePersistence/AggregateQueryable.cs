namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using BuildingBlocks;
    using Environment.MultiTenancy;
    using Microsoft.EntityFrameworkCore;
    
    public class AggregateQueryable<TAggregateRoot> : IQueryable<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private readonly DbContext dbContext;
        private readonly TenantId tenantId;

        public AggregateQueryable(DbContext dbContext, TenantId tenantId)
        {
            this.dbContext = dbContext;
            this.tenantId = tenantId;
        }

        public IEnumerator<TAggregateRoot> GetEnumerator()
        {
            return InnerQueryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)InnerQueryable).GetEnumerator();
        }

        public Type ElementType
        {
            get { return InnerQueryable.ElementType; }
        }

        public Expression Expression
        {
            get { return InnerQueryable.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return InnerQueryable.Provider; }
        }

        private IQueryable<TAggregateRoot> InnerQueryable
        {
            get { return dbContext.Set<TAggregateRoot>().Where(agg => agg.TenantId == tenantId.Value); }
        }
    }
}
