namespace Backend.Fx.BuildingBlocks
{
    using System.Collections.Generic;
    using System.Linq;

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
        void Delete(TAggregateRoot aggregateRoot);
        void Add(TAggregateRoot aggregateRoot);
        void AddRange(TAggregateRoot[] aggregateRoots);
        bool Any();
        TAggregateRoot[] Resolve(IEnumerable<int> ids);
        IQueryable<TAggregateRoot> AggregateQueryable { get; }
        
    }
}
