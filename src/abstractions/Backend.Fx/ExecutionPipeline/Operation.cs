using System;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.ExecutionPipeline
{
    /// <summary>
    /// The basic interface of an operation invoked by the <see cref="IBackendFxApplicationInvoker"/> (or its async counterpart).
    /// Decorate this interface to provide operation specific infrastructure services (like a database connection, a database transaction
    /// an entry-exit logging etc.)
    /// </summary>
    [PublicAPI]
    public interface IOperation
    {
        void Begin(IServiceScope serviceScope);
        
        void Complete();

        void Cancel();
    }

    internal sealed class Operation : IOperation
    {
        private static readonly ILogger Logger = Log.Create<Operation>();
        private static int _index;
        private readonly int _instanceId = _index++;
        private bool? _isActive;
        private IDisposable _lifetimeLogger;

        public void Begin(IServiceScope serviceScope)
        {
            if (_isActive != null)
            {
                throw new InvalidOperationException($"Cannot begin an operation that is {(_isActive.Value ? "active" : "terminated")}");
            }

            _lifetimeLogger = Logger.LogDebugDuration($"Beginning operation #{_instanceId}", $"Terminating operation #{_instanceId}");
            _isActive = true;
        }

        public void Complete()
        {
            Logger.LogInformation("Completing operation #{OperationId}", _instanceId);
            if (_isActive != true)
            {
                throw new InvalidOperationException($"Cannot complete an operation that is {(_isActive == false ? "terminated" : "not active")}");
            }
            
            _isActive = false;
            _lifetimeLogger?.Dispose();
            _lifetimeLogger = null;
        }

        public void Cancel()
        {
            Logger.LogInformation("Canceling operation #{OperationId}", _instanceId);
            if (_isActive != true)
            {
                throw new InvalidOperationException($"Cannot cancel an operation that is {(_isActive == false ? "terminated" : "not active")}");
            }
            _isActive = false;
            _lifetimeLogger?.Dispose();
            _lifetimeLogger = null;
        }
    }
}