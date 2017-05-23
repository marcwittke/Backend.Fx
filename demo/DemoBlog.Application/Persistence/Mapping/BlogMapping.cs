namespace DemoBlog.Persistence.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Backend.Fx.EfCorePersistence;
    using Domain;
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