namespace Backend.Fx.Patterns.UnitOfWork
{
    using System;
    using System.Security.Principal;
    using DependencyInjection;
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
        private static int _index;
        private readonly int _instanceId = _index++;
        private bool? _isCompleted;
        private IDisposable _lifetimeLogger;

        protected ReadonlyUnitOfWork(ICurrentTHolder<IIdentity> identityHolder)
        {
            IdentityHolder = identityHolder;
        }

        public ICurrentTHolder<IIdentity> IdentityHolder { get; }

        public virtual void Begin()
        {
            _lifetimeLogger = Logger.DebugDuration($"Beginning readonly unit of work #{_instanceId}", $"Disposing readonly unit of work #{_instanceId}");
            _isCompleted = false;
        }

        public void Complete()
        {
            Rollback();
            _isCompleted = true;
        }

        public void Dispose()
        {
            if (_isCompleted == false)
            {
                Logger.Info($"Canceling unit of work #{_instanceId} because the instance is being disposed although it did not complete before. This should only occur during cleanup after errors.");
                Rollback();
            }
            _lifetimeLogger?.Dispose();
            _lifetimeLogger = null;
        }

        protected abstract void Rollback();
    }
}