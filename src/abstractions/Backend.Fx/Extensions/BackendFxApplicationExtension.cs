using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Logging;
using Backend.Fx.Util;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Extensions
{
    public abstract class BackendFxApplicationExtension : IBackendFxApplication
    {
        private static readonly ILogger Logger = Log.Create<BackendFxApplicationExtension>();
        private readonly IBackendFxApplication _application;

        protected BackendFxApplicationExtension(IBackendFxApplication application)
        {
            Logger.LogInformation("Decorating the application with {Decorator}", GetType().GetDetailedTypeName());
            _application = application;
        }

        public Assembly[] Assemblies => _application.Assemblies;
        
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

        public TBackendFxApplicationDecorator As<TBackendFxApplicationDecorator>() where TBackendFxApplicationDecorator : BackendFxApplicationExtension
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