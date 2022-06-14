using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.BuildingBlocks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.Authorization;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheDomainModule
    {
        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveInfrastructureServices(CompositionRootType compositionRootType)
        {
            var identity = new SystemIdentity();
            var tenantId = new TenantId(333);

            var sut = new BackendFxTestApplication(compositionRootType);
            await sut.BootAsync();
            
            sut.Invoker.Invoke(
                sp =>
                {
                    var clock = sp.GetRequiredService<IClock>();
                    Assert.IsType<WallClock>(clock);
                    
                    var correlationHolder = sp.GetRequiredService<ICurrentTHolder<Correlation>>();
                    Assert.IsType<CurrentCorrelationHolder>(correlationHolder);
                    
                    var identityHolder = sp.GetRequiredService<ICurrentTHolder<IIdentity>>();
                    Assert.IsType<CurrentIdentityHolder>(identityHolder);
                    Assert.Equal(identity, identityHolder.Current);
                    
                    var tenantIdHolder = sp.GetRequiredService<ICurrentTHolder<TenantId>>();
                    Assert.IsType<CurrentTenantIdHolder>(tenantIdHolder);
                    Assert.Equal(tenantId, tenantIdHolder.Current);
                    
                    var domainEventAggregator = sp.GetRequiredService<IDomainEventAggregator>();
                    Assert.IsType<DomainEventAggregator>(domainEventAggregator);
                },
                identity,
                tenantId);
        }
        
        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveAggregateAuthorization(CompositionRootType compositionRootType)
        {
            var sut = new BackendFxTestApplication(compositionRootType);
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var authorization = sp.GetRequiredService<IAggregateAuthorization<BackendFxAggregate>>();
                    Assert.IsType<AllowAll<BackendFxAggregate>>(authorization);
                },
                new SystemIdentity(),
                new TenantId(333));
        }
        
        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveDomainService(CompositionRootType compositionRootType)
        {
            var sut = new BackendFxTestApplication(compositionRootType);
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var service = sp.GetRequiredService<IAnotherDomainService>();
                    Assert.IsType<AnotherDomainService>(service);
                },
                new SystemIdentity(),
                new TenantId(333));
        }
        
        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task CanResolveApplicationService(CompositionRootType compositionRootType)
        {
            var sut = new BackendFxTestApplication(compositionRootType);
            await sut.BootAsync();

            sut.Invoker.Invoke(
                sp =>
                {
                    var service = sp.GetRequiredService<IAnotherApplicationService>();
                    Assert.IsType<AnotherApplicationService>(service);
                },
                new SystemIdentity(),
                new TenantId(333));
        }
    }
    
    public class BackendFxTestApplication : BackendFxApplication
    {
        public BackendFxTestApplication(CompositionRootType compositionRootType)
            : base(
                compositionRootType.Create(),
                A.Fake<IExceptionLogger>(),
                typeof(BackendFxTestApplication).Assembly)
        {
        }
    }
    
    public class BackendFxAggregate : AggregateRoot
    {
        public string Name { get; set; }

        public BackendFxAggregate(int id, string name) : base(id)
        {
            Name = name;
        }
    }

    public interface IAnotherDomainService
    {
    }

    public class AnotherDomainService : IAnotherDomainService, IDomainService 
    {
    }
    
    public interface IAnotherApplicationService
    {
    }

    public class AnotherApplicationService : IAnotherApplicationService, IDomainService 
    {
    }
}