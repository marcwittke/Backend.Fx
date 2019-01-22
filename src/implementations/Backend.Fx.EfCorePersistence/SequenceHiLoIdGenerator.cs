using System;

namespace Backend.Fx.EfCorePersistence
{
    using Microsoft.EntityFrameworkCore;
    using Patterns.IdGeneration;

    public abstract class SequenceHiLoIdGenerator<TDbContext> : HiLoIdGenerator, IEntityIdGenerator where TDbContext : DbContext
    {
        private readonly ISequence _sequence;
        private readonly Func<TDbContext> _dbContextFactory;

        protected SequenceHiLoIdGenerator(ISequence sequence, Func<TDbContext> dbContextFactory)
        {
            _sequence = sequence;
            _dbContextFactory = dbContextFactory;
        }

        protected override int GetNextBlockStart()
        {
            using (var dbContext = _dbContextFactory())
            {
                return _sequence.GetNextValue(dbContext.Database.GetDbConnection());
            }
        }

        protected override int Increment => _sequence.Increment;
    }
}