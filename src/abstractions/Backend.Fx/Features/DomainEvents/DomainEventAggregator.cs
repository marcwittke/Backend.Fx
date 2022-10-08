using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Features.DomainEvents
{
    public class DomainEventAggregator : IDomainEventAggregator, IDomainEventAggregatorScope
    {
        private static readonly ILogger Logger = Log.Create<DomainEventAggregator>();
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentQueue<IDomainEvent> _domainEvents = new();

        public DomainEventAggregator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Publish a domain event that is handled by all handlers synchronously in the same scope/transaction.
        /// Possible exceptions are not caught, so that your action might fail due to a failing event handler.
        /// </summary>
        /// <typeparam name="TDomainEvent"></typeparam>
        /// <param name="domainEvent"></param>
        public void PublishDomainEvent<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            Logger.LogDebug(
                "Domain event {DomainEvent} registered. It will be raised on completion of operation",
                typeof(TDomainEvent).Name);
            _domainEvents.Enqueue(domainEvent);
        }

        public async Task RaiseEventsAsync(CancellationToken cancellationToken)
        {
            while (_domainEvents.TryDequeue(out IDomainEvent domainEvent))
            {
                Type eventType = domainEvent.GetType();
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
                foreach (object injectedHandler in _serviceProvider.GetServices(handlerType))
                {
                    try
                    {
                        MethodInfo handleMethod = handlerType.GetMethod("HandleAsync");
                        Debug.Assert(handleMethod != null, nameof(handleMethod) + " != null");
                        var task = (Task)handleMethod.Invoke(injectedHandler, new object[] { domainEvent, cancellationToken });
                        await task.ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex,
                            "Handling of {DomainEvent} by {HandlerTypeName} failed",
                            eventType.Name,
                            handlerType.Name);
                        throw;
                    }
                }
            }
        }
    }
}