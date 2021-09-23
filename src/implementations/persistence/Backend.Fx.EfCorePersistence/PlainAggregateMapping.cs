﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Backend.Fx.BuildingBlocks;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCorePersistence
{
    public class PlainAggregateMapping<TAggregateRoot> : AggregateMapping<TAggregateRoot>
        where TAggregateRoot : AggregateRoot
    {
        public override IEnumerable<Expression<Func<TAggregateRoot, object>>> IncludeDefinitions =>
            Array.Empty<Expression<Func<TAggregateRoot, object>>>();

        public override void ApplyEfMapping(ModelBuilder modelBuilder)
        { }
    }
}
