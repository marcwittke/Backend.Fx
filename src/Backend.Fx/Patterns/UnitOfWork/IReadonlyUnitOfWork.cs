namespace Backend.Fx.Patterns.UnitOfWork
{
    using System;
    using System.Security.Principal;
    using Logging;

    /// <summary>
    /// Readonly all-or-nothing operation wrapper, typically implemented by a surrounding database transaction that is always rolled back.
    /// By using this unit of work, you can tell the O/R-mapper not to track changes to save some bytes of memory and some CPU cycles.
    /// </summary>
    public interface IReadonlyUnitOfWork : IUnitOfWork
    { }

    public abstract class ReadonlyUnitOfWork : IReadonlyUnitOfWork
    {
        private static readonly ILogger Logger = LogManager.Create<UnitOfWork>();
        private static int index;
        private readonly int instanceId = index++;
        private bool? isCompleted;
        private IDisposable lifetimeLogger;

        protected ReadonlyUnitOfWork(IIdentity identity)
        {
            Identity = identity;
        }

        public IIdentity Identity { get; }

        public virtual void Begin()
        {
            lifetimeLogger = Logger.DebugDuration($"Beginning readonly unit of work #{instanceId}", $"Disposing readonly unit of work #{instanceId}");
            isCompleted = false;
        }

        public void Complete()
        {
            Rollback();
            isCompleted = true;
        }

        public void Dispose()
        {
            if (isCompleted == false)
            {
                Logger.Info($"Canceling unit of work #{instanceId} because the instance is being disposed although it did not complete before. This should only occur during cleanup after errors.");
                Rollback();
            }
            lifetimeLogger?.Dispose();
            lifetimeLogger = null;
        }

        protected abstract void Rollback();
    }
}