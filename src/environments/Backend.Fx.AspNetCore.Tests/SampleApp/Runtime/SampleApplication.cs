using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Extensions;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Fx.AspNetCore.Tests.SampleApp.Runtime
{
    public class SampleApplication : IBackendFxApplication
    {
        private readonly IBackendFxApplication _application;

        public SampleApplication(ITenantIdProvider tenantIdProvider, IExceptionLogger exceptionLogger)
        {
            var messageBus = new InMemoryMessageBus();
            var compositionRoot = new SimpleInjectorCompositionRoot(messageBus, GetType().Assembly);
            _application = new BackendFxApplication(compositionRoot, messageBus, exceptionLogger);
            _application = new MultiTenantApplication(_application);
            _application = new GenerateDataOnBoot(tenantIdProvider, _application);

            compositionRoot.Container.GetTypesToRegister<ControllerBase>(GetType().Assembly)
                .ForAll(t => compositionRoot.Container.Register(t));
        }

        public void Dispose()
        {
            _application.Dispose();
        }

        public IBackendFxApplicationAsyncInvoker AsyncInvoker => _application.AsyncInvoker;

        public ICompositionRoot CompositionRoot => _application.CompositionRoot;

        public IBackendFxApplicationInvoker Invoker => _application.Invoker;

        public IMessageBus MessageBus => _application.MessageBus;

        public Task BootAsync(CancellationToken cancellationToken = default)
        {
            return _application.BootAsync(cancellationToken);
        }
    }
}
