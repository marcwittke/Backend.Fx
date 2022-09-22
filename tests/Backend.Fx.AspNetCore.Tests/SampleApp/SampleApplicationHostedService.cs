using Backend.Fx.AspNetCore.Bootstrapping;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Logging;
using Backend.Fx.SimpleInjectorDependencyInjection;

namespace Backend.Fx.AspNetCore.Tests.SampleApp
{
    public class SampleApplicationHostedService : BackendFxApplicationHostedService
    {
        public SampleApplicationHostedService(IExceptionLogger exceptionLogger)
        {
            IBackendFxApplication application = new BackendFxApplication(
                new SimpleInjectorCompositionRoot(),
                exceptionLogger,
                typeof(SampleApplicationHostedService).Assembly);

            application = new AspNetCoreApplication(application);

            Application = application;
        }
        
        public  ITenantService TenantService { get; } = new TenantService(new InMemoryTenantRepository());
        
        public override IBackendFxApplication Application { get; }
    }
}