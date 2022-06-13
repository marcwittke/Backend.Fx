using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.SimpleInjectorDependencyInjection;

namespace Backend.Fx.AspNetCore.Tests.SampleApp.Runtime
{
    public class SampleApplication : IBackendFxApplication
    {
        private readonly IBackendFxApplication _application;

        public SampleApplication(ITenantIdProvider tenantIdProvider, IExceptionLogger exceptionLogger)
        {
            ITenantWideMutexManager tenantWideMutexManager = new InMemoryTenantWideMutexManager();
            Assembly domainAssembly = GetType().Assembly;
            
            _application = new BackendFxApplication(
                new SimpleInjectorCompositionRoot(),
                exceptionLogger,
                new SimpleInjectorInfrastructureModule());
            
            _application = new DataGeneratingApplication(
                tenantIdProvider,
                new SimpleInjectorDataGenerationModule(domainAssembly),
                tenantWideMutexManager, _application);
            
            _application.CompositionRoot.RegisterModules(
                new DomainModule(domainAssembly));
        }

        public void Dispose()
        {
            _application.Dispose();
        }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker => _application.AsyncInvoker;

        public ICompositionRoot CompositionRoot => _application.CompositionRoot;

        public IBackendFxApplicationInvoker Invoker => _application.Invoker;

        public bool WaitForBoot(int timeoutMilliSeconds = 2147483647, CancellationToken cancellationToken = default)
        {
            return _application.WaitForBoot(timeoutMilliSeconds, cancellationToken);
        }

        public Task Boot(CancellationToken cancellationToken = default) => BootAsync(cancellationToken);
        public Task BootAsync(CancellationToken cancellationToken = default)
        {
            return _application.BootAsync(cancellationToken);
        }
    }
}