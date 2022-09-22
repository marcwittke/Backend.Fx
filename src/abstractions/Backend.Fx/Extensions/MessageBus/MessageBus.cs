using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Extensions.MessageBus
{
    [PublicAPI]
    public abstract class MessageBus
    {
        private static readonly ILogger Logger = Log.Create<MessageBus>();

        public IBackendFxApplicationInvoker Invoker { get; internal set; }

        public ICompositionRoot CompositionRoot { get; internal set; }

        public abstract void Connect();

        public async Task PublishAsync(IIntegrationEvent integrationEvent)
        {
            var serializer = CompositionRoot.ServiceProvider.GetRequiredService<IIntegrationEventSerializer>();
            SerializedMessage serializedMessage = await serializer.SerializeAsync(integrationEvent).ConfigureAwait(false);
            await PublishMessageAsync(serializedMessage).ConfigureAwait(false);
        }

        public void Subscribe<TEvent>() where TEvent : IIntegrationEvent
        {
            SubscribeToEventMessage(typeof(TEvent).Name);
        }

        public void Subscribe(Type eventType)
        {
            SubscribeToEventMessage(eventType.Name);
        }

        protected async Task ProcessAsync(SerializedMessage serializedMessage)
        {
            Logger.LogInformation("Processing a {EventTypeName} message", serializedMessage.EventTypeName);

            await Invoker.InvokeAsync(async sp =>
            {
                var serializer = sp.GetRequiredService<IIntegrationEventSerializer>();
                var integrationEvent = await serializer.DeserializeAsync(serializedMessage).ConfigureAwait(false);
                var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEvent.GetType());
                var handler = sp.GetRequiredService(handlerType);
                const string methodName = nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync);
                var handleAsyncMethod = handlerType.GetMethod(methodName, new[] { integrationEvent.GetType() });
                Debug.Assert(handleAsyncMethod != null, nameof(handleAsyncMethod) + " != null");
                sp.GetRequiredService<ICurrentTHolder<Correlation>>().Current.Resume(integrationEvent.CorrelationId);
                await ((Task)handleAsyncMethod
                        .Invoke(handler, new object[] { integrationEvent }))
                    .ConfigureAwait(false);
            }, new SystemIdentity()).ConfigureAwait(false);
        }

        

        protected abstract void SubscribeToEventMessage(string eventTypeName);

        protected abstract Task PublishMessageAsync(SerializedMessage serializedMessage);

        protected virtual void Dispose(bool disposing)
        { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}