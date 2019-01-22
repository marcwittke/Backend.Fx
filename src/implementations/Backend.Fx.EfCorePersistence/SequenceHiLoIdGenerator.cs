namespace Backend.Fx.EfCorePersistence
{
    using System.Data;
    using Patterns.IdGeneration;

    public abstract class SequenceHiLoIdGenerator : HiLoIdGenerator, IEntityIdGenerator
    {
        private readonly ISequence _sequence;
        private readonly IDbConnection _dbConnection;
        
        protected SequenceHiLoIdGenerator(ISequence sequence, IDbConnection dbConnection)
        {
            _sequence = sequence;
            _dbConnection = dbConnection;
        }

        protected override int GetNextBlockStart()
        {
            return _sequence.GetNextValue(_dbConnection);
        }

        protected override int BlockSize => _sequence.Increment;
    }
}