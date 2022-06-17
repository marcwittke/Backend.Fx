using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Backend.Fx.BuildingBlocks
{
    /// <summary>
    /// Encapsulates methods for retrieving domain objects 
    /// See https://en.wikipedia.org/wiki/Domain-driven_design#Building_blocks
    /// </summary>
    /// <typeparam name="TAggregateRoot"></typeparam>
    [PublicAPI]
    public interface IAsyncRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        Task<TAggregateRoot> SingleAsync(int id, CancellationToken cancellationToken = default);
        Task<TAggregateRoot> SingleOrDefaultAsync(int id, CancellationToken cancellationToken = default);
        Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(CancellationToken cancellationToken = default);
        Task<TAggregateRoot[]> ResolveAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        IQueryable<TAggregateRoot> AggregateQueryable { get; }
    }
}