using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
        /// The global exception logger of this application
        /// </summary>
        IExceptionLogger ExceptionLogger { get; }

        Assembly[] Assemblies { get; }

        /// <summary>
        /// allows synchronously awaiting application startup
        /// </summary>
        bool WaitForBoot(int timeoutMilliSeconds = int.MaxValue, CancellationToken cancellationToken = default);

        /// <summary>
        /// Initializes and starts the application (async)
        /// </summary>
        /// <returns></returns>
        Task BootAsync(CancellationToken cancellationToken = default);

        TBackendFxApplicationDecorator As<TBackendFxApplicationDecorator>() where TBackendFxApplicationDecorator : BackendFxApplicationDecorator;
    }


    public class BackendFxApplication : IBackendFxApplication
    {
        private static readonly ILogger Logger = Log.Create<BackendFxApplication>();
        private readonly ManualResetEventSlim _isBooted = new ManualResetEventSlim(false);

        /// <summary>
        /// Initializes the application's runtime instance
        /// </summary>
        /// <param name="compositionRoot">The composition root of the dependency injection framework</param>
        /// <param name="exceptionLogger"></param>
        /// <param name="assemblies"></param>
        public BackendFxApplication(ICompositionRoot compositionRoot, IExceptionLogger exceptionLogger, params Assembly[] assemblies)
        {
            assemblies = assemblies ?? Array.Empty<Assembly>();
            
            Logger.LogInformation(
                "Initializing application with {CompositionRoot} providing services from [{Assemblies}]",
                compositionRoot.GetType().GetDetailedTypeName(),
                string.Join(", ", assemblies.Select(ass => ass.GetName().Name)));
            
            var invoker = new BackendFxApplicationInvoker(compositionRoot);
            AsyncInvoker = new ExceptionLoggingAsyncInvoker(exceptionLogger, invoker);
            Invoker = new ExceptionLoggingInvoker(exceptionLogger, invoker);

            CompositionRoot = compositionRoot;
            ExceptionLogger = exceptionLogger;
            Assemblies = assemblies;
            CompositionRoot.RegisterModules(new DomainModule(Assemblies));
        }

        public Assembly[] Assemblies { get; }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker { get; }

        public ICompositionRoot CompositionRoot { get; }

        public IExceptionLogger ExceptionLogger { get; }
        
        public IBackendFxApplicationInvoker Invoker { get; }

        public Task BootAsync(CancellationToken cancellationToken = default)
        {
            Logger.LogInformation("Booting application");
            CompositionRoot.Verify();
            _isBooted.Set();
            return Task.CompletedTask;
        }

        public virtual TBackendFxApplicationDecorator As<TBackendFxApplicationDecorator>()
            where TBackendFxApplicationDecorator : BackendFxApplicationDecorator
        {
            return null;
        }

        public bool WaitForBoot(int timeoutMilliSeconds = int.MaxValue, CancellationToken cancellationToken = default)
        {
            return _isBooted.Wait(timeoutMilliSeconds, cancellationToken);
        }

        public void Dispose()
        {
            Logger.LogInformation("Application shut down initialized");
            CompositionRoot?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}