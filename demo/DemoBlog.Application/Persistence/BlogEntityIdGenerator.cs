namespace DemoBlog.Persistence
{
    using Backend.Fx.EfCorePersistence;
    using Backend.Fx.EfCorePersistence.Mssql;
    using Backend.Fx.Patterns.IdGeneration;
    using Microsoft.EntityFrameworkCore;

    public class BlogEntityIdGenerator : SequenceHiLoIdGenerator<BlogDbContext>, IEntityIdGenerator
    {
        public BlogEntityIdGenerator(DbContextOptions<BlogDbContext> dbContextOptions) : base(new BlogEntityIdSequence(), dbContextOptions)
        { }
    }

    public class BlogEntityIdSequence : MsSqlSequence
    {
        public override int Increment { get; } = 1000;
        protected override string SequenceName { get; } = "SEQ_EntityId";
    }
}
