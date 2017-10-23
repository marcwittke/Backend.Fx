namespace Backend.Fx.EfCorePersistence.Oracle
{
    using Microsoft.EntityFrameworkCore;
    using Patterns.IdGeneration;

    public class OracleSequenceEntityIdGenerator<TDbContext> : OracleSequenceHiLoIdGenerator<TDbContext>, IEntityIdGenerator where TDbContext : DbContext
    {
        public OracleSequenceEntityIdGenerator(DbContextOptions<TDbContext> dbContextOptions) : base(dbContextOptions, "SEQ_ENTITY_ID")
        { }

        protected override int Increment
        {
            get { return 1000; }
        }
    }
}
