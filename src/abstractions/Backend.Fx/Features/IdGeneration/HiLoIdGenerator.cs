using System.Threading;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.IdGeneration
{
    public abstract class HiLoIdGenerator<TId> : IIdGenerator<TId> where TId : struct
    {
        private readonly object _mutex = new();

        public TId NextId()
        {
            lock (_mutex)
            {
                EnsureValidLowAndHiId();
                return GetNextId();
            }
        }

        protected abstract void EnsureValidLowAndHiId();

        protected abstract TId GetNextId();


        protected abstract TId GetNextBlockStart();

        protected abstract TId BlockSize { get; }
    }
    
    [PublicAPI]
    public abstract class HiLoIntIdGenerator : HiLoIdGenerator<int>
    {
        private static readonly ILogger Logger = Log.Create<HiLoIntIdGenerator>();
        private int _highId = -1;
        private int _lowId = -1;
        private readonly bool _isTraceEnabled;

        protected HiLoIntIdGenerator()
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
    
    [PublicAPI]
    public abstract class HiLoLongIdGenerator : HiLoIdGenerator<long>
    {
        private static readonly ILogger Logger = Log.Create<HiLoLongIdGenerator>();
        private long _highId = -1;
        private long _lowId = -1;
        private readonly bool _isTraceEnabled;

        protected HiLoLongIdGenerator()
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

        protected override long GetNextId()
        {
            var nextId = _lowId;
            Interlocked.Increment(ref _lowId);
            if (_isTraceEnabled) Logger.LogTrace("Providing id {NextId}", nextId);
            return nextId;
        }
    }
}