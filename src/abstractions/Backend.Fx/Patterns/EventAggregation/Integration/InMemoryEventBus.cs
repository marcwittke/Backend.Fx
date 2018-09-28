namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System;
    using System.Threading.Tasks;
    using DependencyInjection;
    using Environment.MultiTenancy;
    using Logging;

    public class InMemoryEventBus : EventBus
    {
        private readonly IExceptionLogger _exceptionLogger;

        public InMemoryEventBus(IScopeManager scopeManager, IExceptionLogger exceptionLogger)
                : base(scopeManager, exceptionLogger)
        {
            _exceptionLogger = exceptionLogger;
        }

        public override void Connect()
        { }

        public override Task Publish(IIntegrationEvent integrationEvent)
        {
            // Processing is done on the thread pool and not being awaited. This emulates best the behavior of a real
            // event bus, that incorporates network transfer and another system handling the event
            Task.Run(() =>
            {
                try
                {
                    Process(integrationEvent.GetType().FullName, new InMemoryProcessingContext(integrationEvent));
                }
                catch (Exception ex)
                {
                    _exceptionLogger.LogException(ex);
                }
            });
            return Task.CompletedTask;
        }

        protected override void Subscribe(string eventName)
        { }

        protected override void Unsubscribe(string eventName)
        { }

        private class InMemoryProcessingContext : EventProcessingContext
        {
            private readonly IIntegrationEvent _integrationEvent;

            public InMemoryProcessingContext(IIntegrationEvent integrationEvent)
            {
                _integrationEvent = integrationEvent;
            }

            public override TenantId TenantId => new TenantId(_integrationEvent.TenantId);

            public override dynamic DynamicEvent => _integrationEvent;

            public override IIntegrationEvent GetTypedEvent(Type eventType)
            {
                return _integrationEvent;
            }
        }
    }
}
