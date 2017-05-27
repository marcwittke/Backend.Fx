namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Microsoft.EntityFrameworkCore;

    public class BlogMapping : AggregateRootMapping<Blog>
    {
        public override IEnumerable<Expression<Func<Blog, object>>> IncludeDefinitions
        {
            get { return new Expression<Func<Blog, object>>[] { blog => blog.Subscribers };}
        }
        public override void ApplyEfMapping(ModelBuilder modelBuilder)
        {}
    }
}