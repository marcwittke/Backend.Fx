using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Backend.Fx.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore5Persistence
{
    public interface IAggregateMapping
    {
        void ApplyEfMapping(ModelBuilder modelBuilder);
    }

    public interface IAggregateMapping<T> : IAggregateMapping where T : AggregateRoot
    {
        IEnumerable<Expression<Func<T, object>>> IncludeDefinitions { get; }
    }
}