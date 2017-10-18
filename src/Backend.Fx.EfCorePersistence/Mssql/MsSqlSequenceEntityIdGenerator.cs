namespace Backend.Fx.EfCorePersistence.Mssql
{
    using Microsoft.EntityFrameworkCore;
    using Patterns.IdGeneration;

    public class MsSqlSequenceEntityIdGenerator<TDbContext> : MsSqlSequenceHiLoIdGenerator, IEntityIdGenerator where TDbContext : DbContext
    {
        public MsSqlSequenceEntityIdGenerator(DbContextOptions<TDbContext> dbContextOptions) : base(dbContextOptions, "EntityId")
        { }

        protected override int Increment
        {
            get { return 1000; }
        }
    }
}
