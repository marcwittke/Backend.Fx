using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.Patterns.DependencyInjection
{
    /// <summary>
    /// The root object of the whole backend fx application framework
    /// </summary>
    public interface IBackendFxApplication : IDisposable
    {
        /// <summary>
        /// The async invoker runs a given action asynchronously in an application scope with injection facilities 
        /// </summary>
        IBackendFxApplicationAsyncInvoker AsyncInvoker { get; }

        /// <summary>
        /// The composition root of the dependency injection framework
        /// </summary>
        ICompositionRoot CompositionRoot { get; }

        /// <summary>
        /// The invoker runs a given action in an application scope with injection facilities 
        /// </summary>
        IBackendFxApplicationInvoker Invoker { get; }

        /// <summary>
        /// The message bus to send and receive event messages
        /// </summary>
        IMessageBus MessageBus { get; }

        /// <summary>
        /// allows synchronously awaiting application startup
        /// </summary>
        bool WaitForBoot(int timeoutMilliSeconds = int.MaxValue, CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes and starts the application (async)
        /// </summary>
        /// <returns></returns>
        Task BootAsync(CancellationToken cancellationToken = default);
    }


    public class BackendFxApplication : IBackendFxApplication
    {
        private static readonly ILogger Logger = LogManager.Create<BackendFxApplication>();
        private readonly ManualResetEventSlim _isBooted = new ManualResetEventSlim(false);

        /// <summary>
        /// Initializes the application's runtime instance
        /// </summary>
        /// <param name="compositionRoot">The composition root of the dependency injection framework</param>
        /// <param name="messageBus">The message bus implementation used by this application instance</param>
        /// <param name="exceptionLogger"></param>
        public BackendFxApplication(ICompositionRoot compositionRoot, IMessageBus messageBus, IExceptionLogger exceptionLogger)
        {
            var invoker = new BackendFxApplicationInvoker(compositionRoot);
            AsyncInvoker = new ExceptionLoggingAsyncInvoker(exceptionLogger, invoker);
            Invoker = new ExceptionLoggingInvoker(exceptionLogger, invoker);
            MessageBus = messageBus;
            MessageBus.ProvideInvoker(new SequentializingBackendFxApplicationInvoker(
                                          new WaitForBootInvoker(this, 
                                                                 new ExceptionLoggingAndHandlingInvoker(exceptionLogger, Invoker))));
            CompositionRoot = compositionRoot;
        }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker { get; }

        public ICompositionRoot CompositionRoot { get; }

        public IBackendFxApplicationInvoker Invoker { get; }

        public IMessageBus MessageBus { get; }

        public Task BootAsync(CancellationToken cancellationToken = default)
        {
            Logger.Info("Booting application");
            CompositionRoot.Verify();
            MessageBus.Connect();
            _isBooted.Set();
            return Task.CompletedTask;
        }

        public bool WaitForBoot(int timeoutMilliSeconds = int.MaxValue, CancellationToken cancellationToken = default)
        {
            return _isBooted.Wait(timeoutMilliSeconds, cancellationToken);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                Logger.Info("Application shut down initialized");
                CompositionRoot?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}