using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Backend.Fx.BuildingBlocks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    public class EntityQueryable<TEntity> : IQueryable<TEntity> where TEntity : Entity
    {
        public EntityQueryable([NotNull] DbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        [NotNull]
        public DbContext DbContext { get; }

        private IQueryable<TEntity> InnerQueryable => DbContext.Set<TEntity>();

        public IEnumerator<TEntity> GetEnumerator()
        {
            return InnerQueryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) InnerQueryable).GetEnumerator();
        }

        public Type ElementType => InnerQueryable.ElementType;

        public Expression Expression => InnerQueryable.Expression;

        public IQueryProvider Provider => InnerQueryable.Provider;
    }
}