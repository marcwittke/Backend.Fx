namespace Backend.Fx.Features.IdGeneration
{
    public class SequenceIdGenerator<TId> : IIdGenerator<TId> 
    {
        private readonly ISequence<TId> _sequence;

        public SequenceIdGenerator(ISequence<TId> sequence)
        {
            _sequence = sequence;
        }

        public TId NextId()
        {
            return _sequence.GetNextValue();
        }
    }
}