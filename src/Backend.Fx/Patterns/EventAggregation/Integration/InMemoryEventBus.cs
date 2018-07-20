namespace Backend.Fx.Patterns.EventAggregation.Integration
{
    using System;
    using System.Threading.Tasks;
    using DependencyInjection;
    using Environment.MultiTenancy;
    using Logging;

    public class InMemoryEventBus : EventBus
    {
        private readonly IExceptionLogger exceptionLogger;

        public InMemoryEventBus(IScopeManager scopeManager, IExceptionLogger exceptionLogger)
                : base(scopeManager, exceptionLogger)
        {
            this.exceptionLogger = exceptionLogger;
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
                    exceptionLogger.LogException(ex);
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
            private readonly IIntegrationEvent integrationEvent;

            public InMemoryProcessingContext(IIntegrationEvent integrationEvent)
            {
                this.integrationEvent = integrationEvent;
            }

            public override TenantId TenantId
            {
                get { return new TenantId(integrationEvent.TenantId); }
            }

            public override dynamic DynamicEvent
            {
                get { return integrationEvent; }
            }

            public override IIntegrationEvent GetTypedEvent(Type eventType)
            {
                return integrationEvent;
            }
        }
    }
}
