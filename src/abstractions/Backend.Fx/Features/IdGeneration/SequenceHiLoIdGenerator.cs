using System.Threading;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.IdGeneration
{
    public abstract class SequenceHiLoIdGenerator<TId> : HiLoIdGenerator<TId>
    {
        private readonly ISequence<TId> _sequence;

        protected SequenceHiLoIdGenerator(ISequence<TId> sequence)
        {
            _sequence = sequence;
        }

        protected override TId GetNextBlockStart()
        {
            return _sequence.GetNextValue();
        }

        protected override TId BlockSize => _sequence.Increment;
    }
    
    public class SequenceHiLoIntIdGenerator : SequenceHiLoIdGenerator<int>
    {
        private static readonly ILogger Logger = Log.Create<HiLoIntIdGenerator>();
        private int _highId = -1;
        private int _lowId = -1;
        private readonly bool _isTraceEnabled;
        
        public SequenceHiLoIntIdGenerator(ISequence<int> sequence) : base(sequence)
        {
            _isTraceEnabled = Logger.IsEnabled(LogLevel.Trace);
        }

        protected override void EnsureValidLowAndHiId()
        {
            if (_lowId == -1 || _lowId > _highId)
            {
                // first fetch from sequence in life time
                _lowId = GetNextBlockStart();
                _highId = _lowId + BlockSize- 1;
            }
        }

        protected override int GetNextId()
        {
            var nextId = _lowId;
            Interlocked.Increment(ref _lowId);
            if (_isTraceEnabled) Logger.LogTrace("Providing id {NextId}", nextId);
            return nextId;
        }
    }
}