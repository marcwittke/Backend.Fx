using Backend.Fx.EfCorePersistence.Bootstrapping;
using Backend.Fx.Patterns.IdGeneration;

namespace Backend.Fx.EfCorePersistence
{
    
    public abstract class SequenceHiLoIdGenerator : HiLoIdGenerator, IEntityIdGenerator
    {
        private readonly ISequence _sequence;
        private readonly IDbConnectionFactory _dbConnectionFactory;

        protected SequenceHiLoIdGenerator(ISequence sequence, IDbConnectionFactory dbConnectionFactory)
        {
            _sequence = sequence;
            _dbConnectionFactory = dbConnectionFactory;
        }

        protected override int GetNextBlockStart()
        {
            using (var dbc = _dbConnectionFactory.Create())
            {
                dbc.Open();
                return _sequence.GetNextValue(dbc);
            }
        }

        protected override int BlockSize => _sequence.Increment;
    }
}