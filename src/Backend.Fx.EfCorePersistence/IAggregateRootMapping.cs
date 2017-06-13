namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using BuildingBlocks;
    using Microsoft.EntityFrameworkCore;

    public interface IAggregateRootMapping
    {
        void ApplyEfMapping(ModelBuilder modelBuilder);
    }

    public interface IAggregateRootMapping<T> : IAggregateRootMapping where T : AggregateRoot
    {
         IEnumerable<Expression<Func<T, object>>> IncludeDefinitions { get; }
    }
}
