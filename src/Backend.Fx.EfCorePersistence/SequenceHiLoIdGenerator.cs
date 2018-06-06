namespace Backend.Fx.EfCorePersistence
{
    using Microsoft.EntityFrameworkCore;
    using Patterns.IdGeneration;

    public abstract class SequenceHiLoIdGenerator<TDbContext> : HiLoIdGenerator, IEntityIdGenerator where TDbContext : DbContext
    {
        private readonly ISequence sequence;
        private readonly DbContextOptions<TDbContext> dbContextOptions;

        protected SequenceHiLoIdGenerator(ISequence sequence, DbContextOptions<TDbContext> dbContextOptions)
        {
            this.sequence = sequence;
            this.dbContextOptions = dbContextOptions;
        }

        protected override int GetNextBlockStart()
        {
            using (var dbContext = dbContextOptions.CreateDbContext())
            {
                return sequence.GetNextValue(dbContext);
            }
        }

        protected override int Increment
        {
            get { return sequence.Increment; }
        }
    }
}