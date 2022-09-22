using Backend.Fx.Domain;

namespace Backend.Fx.Extensions.Persistence
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