namespace Backend.Fx.EfCorePersistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using BuildingBlocks;
    using Microsoft.EntityFrameworkCore;

    public class PlainAggregateRootMapping<TAggregateRoot> : AggregateRootMapping<TAggregateRoot>
        where TAggregateRoot : AggregateRoot
    {
        public override void ApplyEfMapping(ModelBuilder modelBuilder)
        { }
        
        public override IEnumerable<Expression<Func<TAggregateRoot, object>>> IncludeDefinitions
        {
            get { return new Expression<Func<TAggregateRoot, object>>[0]; }
        }
    }
}