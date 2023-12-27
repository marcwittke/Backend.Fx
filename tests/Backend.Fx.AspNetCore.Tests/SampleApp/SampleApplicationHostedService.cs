using Backend.Fx.AspNetCore.Tests.SampleApp.Runtime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.EventAggregation.Integration;

namespace Backend.Fx.AspNetCore.Tests.SampleApp
{
    public class SampleApplicationHostedService : BackendFxApplicationHostedService<SampleApplication>
    {
        public  ITenantService TenantService { get; }
        public override SampleApplication Application { get; }

        public SampleApplicationHostedService(IExceptionLogger exceptionLogger)
        {
            IMessageBus messageBus = new InMemoryMessageBus();
            TenantService = new TenantService(messageBus, new InMemoryTenantRepository());
            Application = new SampleApplication(TenantService.TenantIdProvider, exceptionLogger);
        }
    }
}