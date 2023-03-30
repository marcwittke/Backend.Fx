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
        private static readonly ILogger Logger = Log.Create<Operation>();
        private static int _index;
        private readonly int _instanceId = _index++;
        private OperationState _state = OperationState.Initial;
        private IDisposable _lifetimeLogger;

        public void Begin()
        {
            if (_state != OperationState.Initial)
            {
                throw new InvalidOperationException($"Cannot begin operation #{_instanceId} that is {_state}");
            }

            _lifetimeLogger = Logger.LogDebugDuration($"Beginning operation #{_instanceId}", $"Terminating operation #{_instanceId}");
            _state = OperationState.Running;
        }

        public void Complete()
        {
            Logger.LogInformation("Completing operation #{OperationId}", _instanceId);
            if (_state != OperationState.Running)
            {
                throw new InvalidOperationException($"Cannot complete operation #{_instanceId} that is {_state}");
            }
            
            _state = OperationState.Completed;
            _lifetimeLogger?.Dispose();
            _lifetimeLogger = null;
        }

        public void Cancel()
        {
            Logger.LogInformation("Canceling operation #{OperationId}", _instanceId);
            if (_state != OperationState.Running)
            {
                // do not throw in this case, it would just make things worse 
                Logger.LogError("Cannot cancel operation #{OperationId} that is {State}", _instanceId, _state);
            }
            _state = OperationState.Canceled;
            _lifetimeLogger?.Dispose();
            _lifetimeLogger = null;
        }

        private enum OperationState
        {
            Initial,
            Running,
            Completed,
            Canceled
        }
    }
}