namespace Backend.Fx.Patterns.IdGeneration
{
    using System.Threading;
    using Logging;

    public abstract class HiLoIdGenerator : IIdGenerator
    {
        private static readonly ILogger Logger = LogManager.Create<HiLoIdGenerator>();
        private int highId = -1;
        private int lowId = -1;
        private static readonly object Mutex = new object();
        private readonly bool isTraceEnabled;

        protected HiLoIdGenerator()
        {
            isTraceEnabled = Logger.IsTraceEnabled();
        }

        public int NextId()
        {
            lock (Mutex)
            {
                EnsureValidLowAndHiId();
                var nextId = lowId;
                Interlocked.Increment(ref lowId);
                if (isTraceEnabled) Logger.Trace("Providing id {0}", nextId);
                return nextId;
            }
        }

        private void EnsureValidLowAndHiId()
        {
            if (lowId == -1 || lowId > highId)
            {
                // first fetch from sequence in life time
                lowId = GetNextBlockStart();
                highId = lowId + GetSequenceIncrement() - 1;
            }
        }

        protected abstract int GetNextBlockStart();

        protected abstract int GetSequenceIncrement();
    }
}