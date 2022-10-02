using System.Threading.Tasks;
using Backend.Fx.Features.MultiTenancy;
using Backend.Fx.Features.MultiTenancyAdmin;
using Backend.Fx.Features.MultiTenancyAdmin.InMem;
using Backend.Fx.Features.Persistence;
using Backend.Fx.Features.Persistence.InMem;
using Backend.Fx.Logging;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using Backend.Fx.TestUtil;
using FakeItEasy;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Features.Persistence;

public class ThePersistenceFeatureWithMultiTenancy : TestWithLogging
{
    private readonly InMemoryTenantRepository _tenantRepository = new();
    
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();
    private readonly DummyServicesFeature _dummyServicesFeature = new ();

    public ThePersistenceFeatureWithMultiTenancy(ITestOutputHelper output) : base(output)
    {
        _tenantRepository.SaveTenant(new Tenant(1, "t1", "tenant 1", false));
        _tenantRepository.SaveTenant(new Tenant(2, "t2", "tenant 2", false));
        
        _sut = new MultiTenancyBackendFxApplication<DummyTenantIdSelector>(
            new SimpleInjectorCompositionRoot(),
            _exceptionLogger,
            new DirectTenantEnumerator(_tenantRepository), 
            GetType().Assembly);
        
        _sut.EnableFeature(_dummyServicesFeature);
    }

    [Fact]
    public async Task IsolatesTenantsDataFromEachOther()
    {
        _sut.EnableFeature(new PersistenceFeature(new InMemoryPersistenceModule()));
        await _sut.BootAsync();

        DummyTenantIdSelector.TenantId = 100;
        var inTenant100 = new DummyAggregate(1, "one");
        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            await repository.AddAsync(inTenant100);
        });

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            DummyAggregate aggregate = await repository.GetAsync(1);
            Assert.Equal(inTenant100, aggregate);
        });
        
        DummyTenantIdSelector.TenantId = 200;
        var inTenant200 = new DummyAggregate(1, "one");
        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            await repository.AddAsync(inTenant200);
        });

        await _sut.Invoker.InvokeAsync(async sp =>
        {
            var repository = sp.GetRequiredService<IRepository<DummyAggregate, int>>();
            DummyAggregate aggregate = await repository.GetAsync(1);
            Assert.Equal(inTenant200, aggregate);
        });
    }
    
    [UsedImplicitly]
    private class DummyTenantIdSelector : ICurrentTenantIdSelector
    {
        public static int TenantId { get; set; }
        
        public TenantId GetCurrentTenantId()
        {
            return new TenantId(TenantId);
        }
    }
}