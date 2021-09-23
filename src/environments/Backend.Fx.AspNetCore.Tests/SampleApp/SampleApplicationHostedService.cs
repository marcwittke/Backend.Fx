using Backend.Fx.AspNetCore.Tests.SampleApp.Runtime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.AspNetCore.Tests.SampleApp
{
    public class SampleApplicationHostedService : BackendFxApplicationHostedService
    {
        public SampleApplicationHostedService(IExceptionLogger exceptionLogger)
        {
            IMessageBus messageBus = new InMemoryMessageBus();
            TenantService = new TenantService(messageBus, new InMemoryTenantRepository());
            Application = new SampleApplication(new TenantServiceTenantIdProvider(TenantService), exceptionLogger);
        }

        public ITenantService TenantService { get; }

        public override IBackendFxApplication Application { get; }
    }
}
