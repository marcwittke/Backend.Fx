namespace Backend.Fx.EfCorePersistence.Tests.DummyImpl
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Microsoft.EntityFrameworkCore;

    public class BlogMapping : AggregateMapping<Blog>
    {
        public override void ApplyEfMapping(ModelBuilder modelBuilder)
        { }

        public override IEnumerable<Expression<Func<Blog, object>>> IncludeDefinitions
        {
            get
            {
                return new Expression<Func<Blog, object>>[]
                {
                    blog => blog.Posts,
                };
            }
        }

    }
}