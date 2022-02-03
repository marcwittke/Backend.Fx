using System.Threading;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Patterns.IdGeneration
{
    public abstract class HiLoIdGenerator : IIdGenerator
    {
        private static readonly ILogger Logger = LogManager.Create<HiLoIdGenerator>();
        private int _highId = -1;
        private int _lowId = -1;
        private static readonly object Mutex = new object();
        private readonly bool _isTraceEnabled;

        protected HiLoIdGenerator()
        {
            _isTraceEnabled = Logger.IsEnabled(LogLevel.Trace);
        }

        public int NextId()
        {
            lock (Mutex)
            {
                EnsureValidLowAndHiId();
                var nextId = _lowId;
                Interlocked.Increment(ref _lowId);
                if (_isTraceEnabled) Logger.LogTrace("Providing id {NextId}", nextId);
                return nextId;
            }
        }

        private void EnsureValidLowAndHiId()
        {
            if (_lowId == -1 || _lowId > _highId)
            {
                // first fetch from sequence in life time
                _lowId = GetNextBlockStart();
                _highId = _lowId + BlockSize- 1;
            }
        }

        protected abstract int GetNextBlockStart();

        protected abstract int BlockSize { get; }
    }
}