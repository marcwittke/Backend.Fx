using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Logging;

namespace Backend.Fx.Patterns.DependencyInjection
{
    public abstract class BackendFxApplicationDecorator : IBackendFxApplication
    {
        private readonly IBackendFxApplication _application;

        protected BackendFxApplicationDecorator(IBackendFxApplication application)
        {
            _application = application;
        }

        public Assembly[] Assemblies => _application.Assemblies;
        
        public IBackendFxApplicationAsyncInvoker AsyncInvoker => _application.AsyncInvoker;

        public ICompositionRoot CompositionRoot => _application.CompositionRoot;

        public IExceptionLogger ExceptionLogger => _application.ExceptionLogger;

        public IBackendFxApplicationInvoker Invoker => _application.Invoker;

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