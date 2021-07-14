namespace Backend.Fx.Patterns.IdGeneration
{
    public abstract class SequenceIdGenerator : IIdGenerator
    {
        private readonly ISequence _sequence;

        protected SequenceIdGenerator(ISequence sequence)
        {
            _sequence = sequence;
        }

        public int NextId()
        {
            return _sequence.GetNextValue();
        }
    }
}