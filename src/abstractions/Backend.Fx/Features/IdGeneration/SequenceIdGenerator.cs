namespace Backend.Fx.Features.IdGeneration
{
    public class SequenceIdGenerator<TId> : IIdGenerator<TId> where TId : struct
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