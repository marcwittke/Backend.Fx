namespace DemoBlog.Persistence
{
    using Backend.Fx.EfCorePersistence.Mssql;
    using Backend.Fx.Patterns.IdGeneration;
    using Microsoft.EntityFrameworkCore;

    public class BlogEntityIdGenerator : MsSqlSequenceHiLoIdGenerator<BlogDbContext>, IEntityIdGenerator
    {
        public BlogEntityIdGenerator(DbContextOptions<BlogDbContext> dbContextOptions) : base(dbContextOptions, "EntityId")
        { }

        protected override int Increment
        {
            get { return 1000; }
        }
    }
}
