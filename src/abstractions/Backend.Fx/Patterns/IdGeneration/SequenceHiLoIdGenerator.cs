namespace Backend.Fx.Patterns.IdGeneration
{
    public abstract class SequenceHiLoIdGenerator : HiLoIdGenerator
    {
        private readonly ISequence _sequence;

        protected SequenceHiLoIdGenerator(ISequence sequence)
        {
            _sequence = sequence;
        }

        protected override int BlockSize => _sequence.Increment;

        protected override int GetNextBlockStart()
        {
            return _sequence.GetNextValue();
        }
    }
}
