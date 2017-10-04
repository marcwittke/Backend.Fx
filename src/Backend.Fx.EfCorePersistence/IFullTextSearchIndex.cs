namespace Backend.Fx.EfCorePersistence
{
    using Microsoft.EntityFrameworkCore;

    public interface IFullTextSearchIndex
    {
        void EnsureIndex(DbContext dbContext);
    }
}