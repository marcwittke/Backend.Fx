using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.DataGeneration
{
    public class TheDataGenerationContext : TestWithLogging
    {
        private readonly ITenantWideMutexManager _tenantWideMutexManager = A.Fake<ITenantWideMutexManager>();

        public TheDataGenerationContext(ITestOutputHelper output) : base(output)
        {
            var tenantIdProvider = A.Fake<ITenantIdProvider>();
            A.CallTo(() => tenantIdProvider.GetActiveDemonstrationTenantIds()).Returns(_demoTenants);
            A.CallTo(() => tenantIdProvider.GetActiveProductionTenantIds()).Returns(_prodTenants);

            var backendFxApplication =
                new BackendFxApplication(new MicrosoftCompositionRoot(), A.Fake<IExceptionLogger>(),
                    GetType().Assembly);

            _sut = new DataGeneratingApplication(tenantIdProvider, _tenantWideMutexManager, backendFxApplication);
            TestDataGenerator.Calls.Clear();
        }

        private readonly DataGeneratingApplication _sut;

        private readonly TenantId[] _demoTenants = {new TenantId(1), new TenantId(2)};
        private readonly TenantId[] _prodTenants = {new TenantId(11), new TenantId(12)};

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CallsDataGeneratorWhenSeedingForSpecificTenant(bool isDemoTenant)
        {
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

            await _sut.BootAsync();
            _sut.DataGenerationContext.SeedDataForTenant(new TenantId(123), isDemoTenant);

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

        [Fact]
        public async Task DoesNothingWhenCannotAcquireTenantWideMutex()
        {
            ITenantWideMutex m;
            var tryAcquireCall = A.CallTo(() =>
                _tenantWideMutexManager.TryAcquire(
                    A<TenantId>.That.Matches(
                        t => t.Value == 123),
                    A<string>.That.Matches(s => s == "DataGenerationContext"),
                    out m));
            tryAcquireCall.Returns(false);

            await _sut.BootAsync();
            _sut.DataGenerationContext.SeedDataForTenant(new TenantId(123), false);

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