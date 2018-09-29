namespace Backend.Fx.EfCorePersistence
{
    using Microsoft.EntityFrameworkCore;

    public interface ISequence
    {
        void EnsureSequence(DbContext dbContext);
        int GetNextValue(DbContext dbContext);
        int Increment { get; }
    }
}
