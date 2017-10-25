namespace DemoBlog.Persistence
{
    using Backend.Fx.EfCorePersistence;
    using Backend.Fx.EfCorePersistence.Mssql;
    using Backend.Fx.Patterns.IdGeneration;
    using Microsoft.EntityFrameworkCore;

    public class BlogEntityIdGenerator : SequenceHiLoIdGenerator<BlogDbContext>, IEntityIdGenerator
    {
        private static readonly int increment = 1000;

        public BlogEntityIdGenerator(DbContextOptions<BlogDbContext> dbContextOptions) : base(new MsSqlSequence("SEQ_EntityId", increment), dbContextOptions)
        { }
    }
}
