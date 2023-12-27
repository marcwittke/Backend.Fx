using Backend.Fx.AspNetCore.Tests.SampleApp.Runtime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using JetBrains.Annotations;

namespace Backend.Fx.AspNetCore.Tests.SampleApp
{
    [UsedImplicitly]
    public class SampleApplicationHostedService : BackendFxApplicationHostedService<SampleApplication>
    {
        public override SampleApplication Application { get; }

        public SampleApplicationHostedService(IExceptionLogger exceptionLogger, ITenantService tenantService)
        {
            Application = new SampleApplication(tenantService.TenantIdProvider, exceptionLogger);
        }
    }
}