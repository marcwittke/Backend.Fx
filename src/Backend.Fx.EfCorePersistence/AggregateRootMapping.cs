namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using BuildingBlocks;
    using Microsoft.EntityFrameworkCore;

    public abstract class AggregateRootMapping<T> : IAggregateRootMapping<T> where T : AggregateRoot
    {
        public abstract IEnumerable<Expression<Func<T, object>>> IncludeDefinitions { get; }

        public abstract void ApplyEfMapping(ModelBuilder modelBuilder);
    }
}