using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SampleApp.Domain;

namespace Backend.Fx.EfCore5Persistence.Tests.SampleApp.Persistence
{
    public class BlogMapping : AggregateMapping<Blog>
    {
        public override IEnumerable<Expression<Func<Blog, object>>> IncludeDefinitions
        {
            get
            {
                return new Expression<Func<Blog, object>>[]
                {
                    blog => blog.Posts
                };
            }
        }

        public override void ApplyEfMapping(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Post>().OwnsOne(p => p.TargetAudience);
        }
    }
}