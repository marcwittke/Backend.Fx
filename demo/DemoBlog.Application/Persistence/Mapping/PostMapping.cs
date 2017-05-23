namespace DemoBlog.Persistence.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Backend.Fx.EfCorePersistence;
    using Domain;
    using Microsoft.EntityFrameworkCore;

    public class PostMapping : AggregateRootMapping<Post> {
        public override IEnumerable<Expression<Func<Post, object>>> IncludeDefinitions
        {
            get
            {
                return new Expression<Func<Post, object>>[]
                {
                    post => post.Comments,
                };
            }
        }

        public override void ApplyEfMapping(ModelBuilder modelBuilder)
        {
            
        }
    }
}