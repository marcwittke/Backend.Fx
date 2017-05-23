namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using BuildingBlocks;
    using Microsoft.EntityFrameworkCore;

    public interface IAggregateDefinition
    {
        void ApplyEfMapping(ModelBuilder modelBuilder);
    }

    public interface IAggregateRootMapping<T> : IAggregateDefinition where T : AggregateRoot
    {
         IEnumerable<Expression<Func<T, object>>> IncludeDefinitions { get; }
    }
}
