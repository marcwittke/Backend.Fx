namespace Backend.Fx.Patterns.IdGeneration
{
    public class SequenceIdGenerator : IIdGenerator
    {
        private readonly ISequence _sequence;

        public SequenceIdGenerator(ISequence sequence)
        {
            _sequence = sequence;
        }

        public int NextId()
        {
            return _sequence.GetNextValue();
        }
    }
}
