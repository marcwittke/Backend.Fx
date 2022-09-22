﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Backend.Fx.EfCore5Persistence
{
    public abstract class AggregateMapping<T> : IAggregateMapping<T> where T : AggregateRoot
    {
        public abstract IEnumerable<Expression<Func<T, object>>> IncludeDefinitions { get; }

        public abstract void ApplyEfMapping(ModelBuilder modelBuilder);
    }
}