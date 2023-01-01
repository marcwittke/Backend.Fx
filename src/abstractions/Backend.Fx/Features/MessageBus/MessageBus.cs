using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
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
        private readonly ILogger _logger = Log.Create<MessageBus>();
        private readonly ILogger _messageLogger = Log.Create(typeof(MessageBus).FullName + ".Messages");
        private readonly CancellationToken _cancellationToken;

        private IBackendFxApplicationInvoker _invoker;

        private ICompositionRoot _compositionRoot;

        public abstract void Connect();

        public async Task PublishAsync<TIntegrationEvent>(TIntegrationEvent integrationEvent)
            where TIntegrationEvent : IIntegrationEvent
        {
            var serializer = _compositionRoot.ServiceProvider.GetRequiredService<IIntegrationEventMessageSerializer>();
            SerializedMessage serializedMessage =
                await serializer.SerializeAsync(integrationEvent).ConfigureAwait(false);
            
            if (_messageLogger.IsEnabled(LogLevel.Debug))
            {
                _messageLogger.LogDebug("Sending {EventTypeName} payload: {Payload}", serializedMessage.EventTypeName, Encoding.UTF8.GetString(serializedMessage.MessagePayload));
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
            _invoker = new IntegrationEventHandlingInvoker(application.Invoker);
        }

        protected async Task ProcessAsync(SerializedMessage serializedMessage)
        {
            _logger.LogInformation("Processing a {EventTypeName} message", serializedMessage.EventTypeName);

            if (_messageLogger.IsEnabled(LogLevel.Debug))
            {
                _messageLogger.LogDebug("Received {EventTypeName} payload: {Payload}", serializedMessage.EventTypeName, Encoding.UTF8.GetString(serializedMessage.MessagePayload));
            }

            await _invoker.InvokeAsync(async (sp, ct) =>
            {
                var serializer = sp.GetRequiredService<IIntegrationEventMessageSerializer>();
                IIntegrationEvent integrationEvent = await serializer.DeserializeAsync(serializedMessage).ConfigureAwait(false);
                sp.GetRequiredService<ICurrentTHolder<Correlation>>().Current.Resume(integrationEvent.CorrelationId);
                
                Type handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(integrationEvent.GetType());
                Type handlerTypeCollectionType = typeof(IEnumerable<>).MakeGenericType(handlerType);
                var handlers = (IEnumerable)sp.GetRequiredService(handlerTypeCollectionType);
                foreach (var handler in handlers)
                {
                    const string methodName = nameof(IIntegrationEventHandler<IIntegrationEvent>.HandleAsync);
                    MethodInfo handleAsyncMethod = handlerType.GetMethod(methodName, new[] { integrationEvent.GetType(), typeof(CancellationToken) });
                    Debug.Assert(handleAsyncMethod != null, nameof(handleAsyncMethod) + " != null");
                    var task = (Task)handleAsyncMethod.Invoke(handler, new object[] { integrationEvent, ct });
                    await task.ConfigureAwait(false);
                }
                
            }, new SystemIdentity(), _cancellationToken).ConfigureAwait(false);
        }


        protected abstract void SubscribeToEventMessage(string eventTypeName);

        protected abstract Task PublishMessageAsync(SerializedMessage serializedMessage);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}