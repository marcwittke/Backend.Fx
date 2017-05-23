namespace Backend.Fx.BuildingBlocks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Encapsulates methods for retrieving domain objects 
    /// See https://en.wikipedia.org/wiki/Domain-driven_design#Building_blocks
    /// </summary>
    /// <typeparam name="TAggregateRoot"></typeparam>
    public interface IRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        TAggregateRoot Single(int id);
        TAggregateRoot SingleOrDefault(int id);
        TAggregateRoot[] GetAll();
        void Delete(int id);
        void Delete(TAggregateRoot aggregateRoot);
        void Add(TAggregateRoot aggregateRoot);
        bool Any();
        TAggregateRoot[] Where(Expression<Func<TAggregateRoot, bool>> predicate);
        TAggregateRoot[] Resolve(IEnumerable<int> ids);
        IQueryable<TAggregateRoot> AggregateQueryable { get; }
    }
}
