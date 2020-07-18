namespace Backend.Fx.Patterns.UnitOfWork
{
    using System;
    using System.Security.Principal;
    using DependencyInjection;
    using Environment.DateAndTime;
    using EventAggregation.Domain;
    using EventAggregation.Integration;
    using Extensions;
    using Logging;

    /// <summary>
    /// Maintains a list of objects affected by a business transaction and coordinates the writing out of changes and the resolution of concurrency problems.
    /// </summary>
    public interface IUnitOfWork 
    {
        void Begin();
        void Complete();
        ICurrentTHolder<IIdentity> IdentityHolder { get; }
    }

    public abstract class UnitOfWork : IUnitOfWork, ICanFlush
    {
        private static readonly ILogger Logger = LogManager.Create<UnitOfWork>();
        private static int _index;
        private readonly int _instanceId = _index++;
        private readonly IDomainEventAggregator _eventAggregator;
        private readonly IEventBusScope _eventBusScope;
        private bool? _isCompleted;
        private IDisposable _lifetimeLogger;

        protected UnitOfWork(IClock clock, ICurrentTHolder<IIdentity> identityHolder,
                             IDomainEventAggregator eventAggregator, IEventBusScope eventBusScope)
        {
            Clock = clock;
            _eventAggregator = eventAggregator;
            _eventBusScope = eventBusScope;
            IdentityHolder = identityHolder;
        }

        public ICurrentTHolder<IIdentity> IdentityHolder { get; }
        public IClock Clock { get; }

        public virtual void Flush()
        {
            Logger.Debug("Flushing unit of work #" + _instanceId);
        }
    
        public virtual void Begin()
        {
            _lifetimeLogger = Logger.DebugDuration($"Beginning unit of work #{_instanceId}", $"Disposing unit of work #{_instanceId}");
            _isCompleted = false;
        }
        
        public virtual void Complete()
        {
            Logger.Debug("Completing unit of work #" + _instanceId);
            Flush(); // we have to flush before raising events, therefore the handlers find the latest changes in the DB
            _eventAggregator.RaiseEvents();
            Flush(); // event handlers change the DB state, so we have to flush again
            AsyncHelper.RunSync(()=>_eventBusScope.RaiseEvents());
            _isCompleted = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_isCompleted == false)
                {
                    Logger.Info($"Canceling unit of work #{_instanceId}.");
                }
                _lifetimeLogger?.Dispose();
                _lifetimeLogger = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
