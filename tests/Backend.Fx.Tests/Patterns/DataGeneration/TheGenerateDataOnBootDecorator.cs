using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Hacking;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.DataGeneration
{
    public class TheGenerateDataOnBootDecorator : TestWithLogging
    {
        public TheGenerateDataOnBootDecorator(ITestOutputHelper output) : base(output)
        {
            ITenantWideMutexManager tenantWideMutexManager = new InMemoryTenantWideMutexManager();
            A.Fake<ICompositionRoot>();
            _dataGenerationContext = A.Fake<IDataGenerationContext>();
            _tenantIdProvider = A.Fake<ITenantIdProvider>();

            var backendFxApplication = A.Fake<IBackendFxApplication>();
            _sut = new DataGeneratingApplication(_tenantIdProvider, tenantWideMutexManager, backendFxApplication);

            _sut.SetPrivate(f => f.DataGenerationContext, _dataGenerationContext);
        }

        private readonly DataGeneratingApplication _sut;
        private readonly IDataGenerationContext _dataGenerationContext;
        private readonly ITenantIdProvider _tenantIdProvider;

        [Fact]
        public void DelegatesAllOtherCalls()
        {
            var app = A.Fake<IBackendFxApplication>();
            
            IBackendFxApplication sut = new DataGeneratingApplication(
                A.Fake<ITenantIdProvider>(),
                new InMemoryTenantWideMutexManager(),
                app);

            // ReSharper disable UnusedVariable
            IBackendFxApplicationAsyncInvoker ai = sut.AsyncInvoker;
            A.CallTo(() => app.AsyncInvoker).MustHaveHappenedOnceExactly();

            ICompositionRoot cr = sut.CompositionRoot;
            A.CallTo(() => app.CompositionRoot).MustHaveHappenedOnceOrMore();

            sut.Dispose();
            A.CallTo(() => app.Dispose()).MustHaveHappenedOnceExactly();

            IBackendFxApplicationInvoker i = sut.Invoker;
            A.CallTo(() => app.Invoker).MustHaveHappenedOnceOrMore();
            // ReSharper restore UnusedVariable
        }


        [Fact]
        public async Task RunsDataGeneratorsOnBoot()
        {
            var tenantId1 = new TenantId(1);
            var tenantId2 = new TenantId(2);
            var tenantId3 = new TenantId(3);
            var tenantId4 = new TenantId(4);

            A.CallTo(() => _tenantIdProvider.GetActiveDemonstrationTenantIds()).Returns(new[] {tenantId1, tenantId2});
            A.CallTo(() => _tenantIdProvider.GetActiveProductionTenantIds()).Returns(new[] {tenantId3, tenantId4});
            await _sut.BootAsync();
            A.CallTo(() =>
                _dataGenerationContext.SeedDataForTenant(
                    A<TenantId>.That.IsEqualTo(tenantId1),
                    A<bool>.That.IsEqualTo(true))).MustHaveHappenedOnceExactly();
            A.CallTo(() =>
                _dataGenerationContext.SeedDataForTenant(
                    A<TenantId>.That.IsEqualTo(tenantId2),
                    A<bool>.That.IsEqualTo(true))).MustHaveHappenedOnceExactly();
            A.CallTo(() =>
                _dataGenerationContext.SeedDataForTenant(
                    A<TenantId>.That.IsEqualTo(tenantId3),
                    A<bool>.That.IsEqualTo(false))).MustHaveHappenedOnceExactly();
            A.CallTo(() =>
                _dataGenerationContext.SeedDataForTenant(
                    A<TenantId>.That.IsEqualTo(tenantId4),
                    A<bool>.That.IsEqualTo(false))).MustHaveHappenedOnceExactly();
        }
    }
}