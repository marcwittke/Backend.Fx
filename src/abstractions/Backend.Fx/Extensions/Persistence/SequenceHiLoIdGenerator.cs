﻿namespace Backend.Fx.Extensions.Persistence
{
    public class SequenceHiLoIdGenerator : HiLoIdGenerator
    {
        private readonly ISequence _sequence;

        public SequenceHiLoIdGenerator(ISequence sequence)
        {
            _sequence = sequence;
        }

        protected override int GetNextBlockStart()
        {
            return _sequence.GetNextValue();
        }

        protected override int BlockSize => _sequence.Increment;
    }
}