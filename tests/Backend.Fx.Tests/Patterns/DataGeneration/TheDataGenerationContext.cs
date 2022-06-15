using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.DataGeneration
{
    public class TheDataGenerationContext : TestWithLogging
    {
        private readonly TenantId[] _demoTenants = { new TenantId(1), new TenantId(2) };
        private readonly TenantId[] _prodTenants = { new TenantId(11), new TenantId(12) };
        private readonly ITenantIdProvider _tenantIdProvider = A.Fake<ITenantIdProvider>();
        private readonly ITenantWideMutexManager _tenantWideMutexManager = A.Fake<ITenantWideMutexManager>();

        public TheDataGenerationContext(ITestOutputHelper output) : base(output)
        {
            A.CallTo(() => _tenantIdProvider.GetActiveDemonstrationTenantIds()).Returns(_demoTenants);
            A.CallTo(() => _tenantIdProvider.GetActiveProductionTenantIds()).Returns(_prodTenants);

            TestDataGenerator.Calls.Clear();
        }


        [Theory]
        [InlineData(CompositionRootType.Microsoft, true)]
        [InlineData(CompositionRootType.Microsoft, false)]
        [InlineData(CompositionRootType.SimpleInjector, true)]
        [InlineData(CompositionRootType.SimpleInjector, false)]
        public async Task CallsDataGeneratorWhenSeedingForSpecificTenant(CompositionRootType compositionRootType, bool isDemoTenant)
        {
            var backendFxApplication =
                new BackendFxApplication(
                    compositionRootType.Create(), 
                    A.Fake<IExceptionLogger>(),
                    GetType().Assembly);

            using var sut = new DataGeneratingApplication(_tenantIdProvider, _tenantWideMutexManager, backendFxApplication);
            
            ITenantWideMutex disposable = A.Fake<ITenantWideMutex>();
            ITenantWideMutex m;
            var tryAcquireCall = A.CallTo(() =>
                _tenantWideMutexManager.TryAcquire(
                    A<TenantId>.That.Matches(
                        t => t.Value == 123),
                    A<string>.That.Matches(s => s == "DataGenerationContext"),
                    out m));
            tryAcquireCall
                .Returns(true)
                .AssignsOutAndRefParameters(disposable);

            await sut.BootAsync();
            sut.DataGenerationContext.SeedDataForTenant(new TenantId(123), isDemoTenant);

            Assert.Contains(nameof(ProdDataGenerator1), TestDataGenerator.Calls);

            if (isDemoTenant)
            {
                Assert.Contains(nameof(DemoDataGenerator1), TestDataGenerator.Calls);
                Assert.Contains(nameof(DemoDataGenerator2), TestDataGenerator.Calls);
            }
            else
            {
                Assert.DoesNotContain(nameof(DemoDataGenerator1), TestDataGenerator.Calls);
                Assert.DoesNotContain(nameof(DemoDataGenerator2), TestDataGenerator.Calls);
            }

            tryAcquireCall.MustHaveHappenedOnceExactly();
            A.CallTo(() => disposable.Dispose()).MustHaveHappenedOnceExactly();
        }

        [Theory]
        [InlineData(CompositionRootType.SimpleInjector)]
        [InlineData(CompositionRootType.Microsoft)]
        public async Task DoesNothingWhenCannotAcquireTenantWideMutex(CompositionRootType compositionRootType)
        {
            ITenantWideMutex m;
            var tryAcquireCall = A.CallTo(() =>
                _tenantWideMutexManager.TryAcquire(
                    A<TenantId>.That.Matches(
                        t => t.Value == 123),
                    A<string>.That.Matches(s => s == "DataGenerationContext"),
                    out m));
            tryAcquireCall.Returns(false);

            var backendFxApplication =
                new BackendFxApplication(
                    compositionRootType.Create(),
                    A.Fake<IExceptionLogger>(),
                    GetType().Assembly);

            using var sut = new DataGeneratingApplication(_tenantIdProvider, _tenantWideMutexManager, backendFxApplication);
            await sut.BootAsync();
            sut.DataGenerationContext.SeedDataForTenant(new TenantId(123), false);

            Assert.Empty(TestDataGenerator.Calls);

            tryAcquireCall.MustHaveHappenedOnceExactly();
        }

        private class DemoDataGenerator1 : TestDataGenerator, IDemoDataGenerator
        {
        }

        private class DemoDataGenerator2 : TestDataGenerator, IDemoDataGenerator
        {
        }

        private class ProdDataGenerator1 : TestDataGenerator, IProductiveDataGenerator
        {
        }

        private abstract class TestDataGenerator
        {
            public static List<string> Calls = new List<string>();

            public int Priority => 0;

            public void Generate()
            {
                Calls.Add(GetType().Name);
            }
        }
    }
}