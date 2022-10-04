using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.MessageBus
{
    [PublicAPI]
    public interface IMessageBus : IDisposable
    {
        void Connect();

        Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent)
            where TIntegrationEvent : IIntegrationEvent;

        void Subscribe(Type eventType);
        void Integrate(IBackendFxApplication application);
    }

    public abstract class MessageBus : IMessageBus
    {
        private static readonly ILogger Logger = Log.Create<MessageBus>();
        private static readonly ILogger MessageLogger = Log.Create(typeof(MessageBus).FullName + ".Messages");

        private IBackendFxApplicationInvoker _invoker;

        private ICompositionRoot _compositionRoot;

        public abstract void Connect();

        public async Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent)
            where TIntegrationEvent : IIntegrationEvent
        {
            var serializer = _compositionRoot.ServiceProvider.GetRequiredService<IIntegrationEventMessageSerializer>();
            SerializedMessage serializedMessage =
                await serializer.SerializeAsync(integrationEvent).ConfigureAwait(false);
            
            if (MessageLogger.IsEnabled(LogLevel.Debug))
            {
                MessageLogger.LogDebug("Sending {EventTypeName} payload: {Payload}", serializedMessage.EventTypeName, Encoding.UTF8.GetString(serializedMessage.MessagePayload));
            }
            
            await PublishMessageAsync(serializedMessage).ConfigureAwait(false);
        }

        public void Subscribe(Type eventType)
        {
            SubscribeToEventMessage(eventType.Name);
        }

        public void Integrate(IBackendFxApplication application)
        {
            _compositionRoot = application.CompositionRoot;
            _invoker = new IntegrationEventHandlingInvoker(application.ExceptionLogger, application.Invoker);
        }

        protected async Task ProcessAsync(SerializedMessage serializedMessage)
        {
            Logger.LogInformation("Processing a {EventTypeName} message", serializedMessage.EventTypeName);

            if (MessageLogger.IsEnabled(LogLevel.Debug))
            {
                MessageLogger.LogDebug("Received {EventTypeName} payload: {Payload}", serializedMessage.EventTypeName, Encoding.UTF8.GetString(serializedMessage.MessagePayload));
            }

            await _invoker.InvokeAsync(async sp =>
            {
                var serializer = sp.GetRequiredService<IIntegrationEventMessageSerializer>();
                var integrationEvent = await serializer.DeserializeAsync(serializedMessage).ConfigureAwait(false);
                var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEvent.GetType());
                var handlerTypeCollectionType = typeof(IEnumerable<>).MakeGenericType(handlerType);
                var handlers = (IEnumerable)sp.GetRequiredService(handlerTypeCollectionType);
                foreach (var handler in handlers)
                {
                    const string methodName = nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync);
                    var handleAsyncMethod = handlerType.GetMethod(methodName, new[] { integrationEvent.GetType() });
                    Debug.Assert(handleAsyncMethod != null, nameof(handleAsyncMethod) + " != null");
                    sp.GetRequiredService<ICurrentTHolder<Correlation>>().Current.Resume(integrationEvent.CorrelationId);

                    var task = (Task)handleAsyncMethod.Invoke(handler, new object[] { integrationEvent });
                    await task.ConfigureAwait(false);
                }
                
            }, new SystemIdentity()).ConfigureAwait(false);
        }


        protected abstract void SubscribeToEventMessage(string eventTypeName);

        protected abstract Task PublishMessageAsync(SerializedMessage serializedMessage);

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}