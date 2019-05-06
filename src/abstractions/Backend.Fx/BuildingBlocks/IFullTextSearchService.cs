
namespace Backend.Fx.BuildingBlocks
{
    using System.Linq;

    public interface IFullTextSearchService<out TAggregateRoot>  where TAggregateRoot : AggregateRoot
    {
        IQueryable<TAggregateRoot> Search(string searchQuery);
    }
}
