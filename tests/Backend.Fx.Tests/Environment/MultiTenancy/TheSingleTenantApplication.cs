using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.InMemoryPersistence;
using Backend.Fx.Logging;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.Patterns.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Environment.MultiTenancy
{
    public class TheSingleTenantApplication : TestWithLogging
    {
        private readonly SingleTenantApplication _sut;
        private readonly ITenantRepository _tenantRepository = new InMemoryTenantRepository();

        public TheSingleTenantApplication(ITestOutputHelper output) : base(output)
        {
            _sut = new SingleTenantApplication(
                _tenantRepository,
                false,
                new BackendFxApplication(new MicrosoftCompositionRoot(), new ExceptionLoggers(),
                    typeof(TheSingleTenantApplication).Assembly));
        }

        [Fact]
        public void CreatesTenantOnBootWhenNotExistent()
        {
            var tenants = _tenantRepository.GetTenants();
            Assert.Empty(tenants);

            _sut.Boot();

            tenants = _tenantRepository.GetTenants();
            Assert.Single(tenants);
        }

        [Fact]
        public void CreatesNoTenantOnBootWhenExistent()
        {
            var tenant = new Tenant("single tenant", "", false);
            _tenantRepository.SaveTenant(tenant);

            _sut.Boot();

            var tenants = _tenantRepository.GetTenants();
            Assert.Single(tenants);

            Assert.Equal(tenant.Id, tenants[0].Id);
        }

        [Fact]
        public void CreatesOnlyOneTenantEvenWhenBootedMultipleTimes()
        {
            var tenants = _tenantRepository.GetTenants();
            Assert.Empty(tenants);

            _sut.Boot();
            _sut.Boot();
            _sut.Boot();
            _sut.Boot();

            tenants = _tenantRepository.GetTenants();
            Assert.Single(tenants);
        }

        [Fact]
        public void CreatesOnlyOneTenantEvenWhenBootedMultipleTimesInParallel()
        {
            var tenants = _tenantRepository.GetTenants();
            Assert.Empty(tenants);

            Task.WaitAll(
                Task.Run(() => _sut.Boot()),
                Task.Run(() => _sut.Boot()),
                Task.Run(() => _sut.Boot()),
                Task.Run(() => _sut.Boot())
            );


            tenants = _tenantRepository.GetTenants();
            Assert.Single(tenants);
        }
    }
}