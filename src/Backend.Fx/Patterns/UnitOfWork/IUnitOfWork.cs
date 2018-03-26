namespace Backend.Fx.Patterns.UnitOfWork
{
    using System;
    using System.Security.Principal;
    using DependencyInjection;
    using Environment.DateAndTime;
    using Logging;

    /// <summary>
    /// All-or-nothing operation wrapper, typically implemented by a surrounding database transaction
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        void Begin();
        void Complete();
        ICurrentTHolder<IIdentity> IdentityHolder { get; }
    }

    public abstract class UnitOfWork : IUnitOfWork, ICanFlush
    {
        private static readonly ILogger Logger = LogManager.Create<UnitOfWork>();
        private static int index;
        private readonly int instanceId = index++;
        private readonly IClock clock;
        private bool? isCompleted;
        private IDisposable lifetimeLogger;

        protected UnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder)
        {
            this.clock = clock;
            this.IdentityHolder = identityHolder;
        }

        public ICurrentTHolder<IIdentity> IdentityHolder { get; }

        public virtual void Flush()
        {
            Logger.Debug("Flushing unit of work #" + instanceId);
            UpdateTrackingProperties(IdentityHolder.Current.Name, clock.UtcNow);
        }
        
        public virtual void Begin()
        {
            lifetimeLogger = Logger.DebugDuration($"Beginning unit of work #{instanceId}", $"Disposing unit of work #{instanceId}");
            isCompleted = false;
        }

        public void Complete()
        {
            Logger.Debug("Completing unit of work #" + instanceId);
            UpdateTrackingProperties(IdentityHolder.Current.Name, clock.UtcNow);
            Commit();
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

        protected abstract void UpdateTrackingProperties(string userId, DateTime utcNow);
        protected abstract void Commit();
        protected abstract void Rollback();
    }
}
