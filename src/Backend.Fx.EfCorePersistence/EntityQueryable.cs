namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using BuildingBlocks;
    using Microsoft.EntityFrameworkCore;
    
    public class EntityQueryable<TEntity> : IQueryable<TEntity> where TEntity : Entity
    {
        private readonly DbContext dbContext;
        
        public EntityQueryable(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IEnumerator<TEntity> GetEnumerator()
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

        private IQueryable<TEntity> InnerQueryable
        {
            get
            {
                return dbContext.Set<TEntity>();
            }
        }
    }
}
