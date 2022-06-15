using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public abstract class BackendFxApplicationDecorator : IBackendFxApplication
    {
        private static readonly ILogger Logger = Log.Create<BackendFxApplicationDecorator>();
        private readonly IBackendFxApplication _application;

        protected BackendFxApplicationDecorator(IBackendFxApplication application)
        {
            Logger.LogInformation("Decorating the application with {Decorator}", GetType().GetDetailedTypeName());
            _application = application;
        }

        public Assembly[] Assemblies => _application.Assemblies;
        
        public virtual IBackendFxApplicationAsyncInvoker AsyncInvoker => _application.AsyncInvoker;

        public virtual ICompositionRoot CompositionRoot => _application.CompositionRoot;

        public virtual IExceptionLogger ExceptionLogger => _application.ExceptionLogger;

        public virtual IBackendFxApplicationInvoker Invoker => _application.Invoker;

        public virtual bool WaitForBoot(
            int timeoutMilliSeconds = int.MaxValue,
            CancellationToken cancellationToken = default)
        {
            return _application.WaitForBoot(timeoutMilliSeconds, cancellationToken);
        }

        public virtual Task BootAsync(CancellationToken cancellationToken = default)
        {
            return _application.BootAsync(cancellationToken);
        }

        public TBackendFxApplicationDecorator As<TBackendFxApplicationDecorator>() where TBackendFxApplicationDecorator : BackendFxApplicationDecorator
        {
            if (this is TBackendFxApplicationDecorator matchingDecorator)
            {
                return matchingDecorator;
            }

            return _application.As<TBackendFxApplicationDecorator>();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _application?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}