using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Backend.Fx.Domain;
using Backend.Fx.Features.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore6Persistence;

public class EfCoreAggregateQueryable<TAggregateRoot, TId> : IAggregateQueryable<TAggregateRoot, TId>
    where TAggregateRoot : class, IAggregateRoot<TId>
    where TId : struct, IEquatable<TId>
{
    private readonly IQueryable<TAggregateRoot> _dbSet;
    

    public EfCoreAggregateQueryable(DbContext dbContext)
    {
        _dbSet = dbContext.Set<TAggregateRoot>();
    }

    public Type ElementType => typeof(TAggregateRoot);
    
    public Expression Expression => _dbSet.AsQueryable().Expression;
    
    IQueryProvider IQueryable.Provider => _dbSet.Provider;

    public IEnumerator<TAggregateRoot> GetEnumerator()
    {
        return _dbSet.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_dbSet).GetEnumerator();
    }
}