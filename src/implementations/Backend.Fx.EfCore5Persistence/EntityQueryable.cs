using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore5Persistence
{
    [PublicAPI]
    public class EntityQueryable<TEntity> : IQueryable<TEntity> where TEntity : Entity
    {
        [CanBeNull] private DbContext _dbContext;

        public EntityQueryable(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public DbContext DbContext
        {
            get => _dbContext ?? throw new InvalidOperationException(
                       "This EntityQueryable does not have a DbContext yet. You might either make sure a proper DbContext gets injected or the DbContext is initialized later using a derived class")
            ;
            protected set
            {
                if (_dbContext != null) throw new InvalidOperationException("This EntityQueryable has already a DbContext assigned. It is not allowed to change it later.");
                _dbContext = value;
            }
        }

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