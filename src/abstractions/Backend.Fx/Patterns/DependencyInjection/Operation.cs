using System;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Patterns.DependencyInjection
{
    /// <summary>
    /// The basic interface of an operation invoked by the <see cref="IBackendFxApplicationInvoker"/> (or its async counterpart).
    /// Decorate this interface to provide operation specific infrastructure services (like a database connection, a database transaction
    /// an entry-exit logging etc.)
    /// </summary>
    public interface IOperation
    {
        void Begin();
        
        void Complete();

        void Cancel();
    }

    public class Operation : IOperation
    {
        private static readonly ILogger Logger = LogManager.Create<Operation>();
        private static int _index;
        private readonly int _instanceId = _index++;
        private bool? _isActive;
        private IDisposable _lifetimeLogger;

        public virtual void Begin()
        {
            if (_isActive != null)
            {
                throw new InvalidOperationException($"Cannot begin an operation that is {(_isActive.Value ? "active" : "terminated")}");
            }

            _lifetimeLogger = Logger.LogDebugDuration($"Beginning operation #{_instanceId}", $"Terminating operation #{_instanceId}");
            _isActive = true;
        }

        public virtual void Complete()
        {
            Logger.LogInformation("Completing operation #{OperationId}", _instanceId);
            if (_isActive != true)
            {
                throw new InvalidOperationException($"Cannot begin an operation that is {(_isActive == false ? "terminated" : "not active")}");
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