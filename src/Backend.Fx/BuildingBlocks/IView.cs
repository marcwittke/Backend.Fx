namespace Backend.Fx.BuildingBlocks
{
    using System.Linq;

    public interface IView<out T> : IQueryable<T>
    {
    }
}
