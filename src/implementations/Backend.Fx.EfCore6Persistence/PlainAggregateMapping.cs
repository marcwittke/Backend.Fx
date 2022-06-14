using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Backend.Fx.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore6Persistence
{
    public class PlainAggregateMapping<TAggregateRoot> : AggregateMapping<TAggregateRoot>
        where TAggregateRoot : AggregateRoot
    {
        public override IEnumerable<Expression<Func<TAggregateRoot, object>>> IncludeDefinitions => new Expression<Func<TAggregateRoot, object>>[0];

        public override void ApplyEfMapping(ModelBuilder modelBuilder)
        {
        }
    }
}