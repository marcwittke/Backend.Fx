using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.SimpleInjectorDependencyInjection.Modules;

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
                new InMemoryMessageBus(),
                exceptionLogger);
            _application = new GenerateDataOnTenantCreated(_application, tenantWideMutexManager);
            _application = new GenerateDataOnBoot(
                tenantIdProvider,
                new SimpleInjectorDataGenerationModule(domainAssembly), 
                _application,
                tenantWideMutexManager);
            _application.CompositionRoot.RegisterModules(
                new SimpleInjectorDomainModule(domainAssembly));
        }

        public void Dispose()
        {
            _application.Dispose();
        }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker => _application.AsyncInvoker;

        public ICompositionRoot CompositionRoot => _application.CompositionRoot;

        public IBackendFxApplicationInvoker Invoker => _application.Invoker;

        public IMessageBus MessageBus => _application.MessageBus;

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