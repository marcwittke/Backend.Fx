using System;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.ExecutionPipeline
{
    /// <summary>
    /// The basic interface of an operation invoked by the <see cref="IBackendFxApplicationInvoker"/>.
    /// Decorate this interface to provide operation specific infrastructure services (like a database connection, a database transaction
    /// an entry-exit logging etc.)
    /// </summary>
    [PublicAPI]
    public interface IOperation
    {
        Task BeginAsync(IServiceScope serviceScope, CancellationToken cancellationToken = default);
        
        Task CompleteAsync(CancellationToken cancellationToken = default);

        Task CancelAsync(CancellationToken cancellationToken = default);
    }

    
    [UsedImplicitly]
    internal sealed class Operation : IOperation
    {
        private static readonly ILogger Logger = Log.Create<Operation>();
        private readonly int _instanceId;
        private bool? _isActive;
        private IDisposable _lifetimeLogger;

        public Operation(OperationCounter operationCounter)
        {
            _instanceId = operationCounter.Count();
        }
        
        public Task BeginAsync(IServiceScope serviceScope, CancellationToken cancellationToken = default)
        {
            if (_isActive != null)
            {
                throw new InvalidOperationException($"Cannot begin an operation that is {(_isActive.Value ? "active" : "terminated")}");
            }

            _lifetimeLogger = Logger.LogDebugDuration($"Beginning operation #{_instanceId}", $"Terminating operation #{_instanceId}");
            _isActive = true;
            return Task.CompletedTask;
        }

        public Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Completing operation #{OperationId}", _instanceId);
            if (_isActive != true)
            {
                throw new InvalidOperationException($"Cannot complete an operation that is {(_isActive == false ? "terminated" : "not active")}");
            }
            
            _isActive = false;
            _lifetimeLogger?.Dispose();
            _lifetimeLogger = null;
            return Task.CompletedTask;
        }

        public Task CancelAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Canceling operation #{OperationId}", _instanceId);
            _isActive = false;
            _lifetimeLogger?.Dispose();
            _lifetimeLogger = null;
            return Task.CompletedTask;
        }
    }
}