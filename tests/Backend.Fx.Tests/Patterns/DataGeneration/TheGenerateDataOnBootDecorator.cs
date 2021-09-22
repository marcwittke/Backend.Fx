using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Hacking;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DataGeneration
{
    public class TheGenerateDataOnBootDecorator
    {
        private readonly ICompositionRoot _compositionRoot;
        private readonly IDataGenerationContext _dataGenerationContext;
        private readonly IModule _dataGenerationModule;

        private readonly GenerateDataOnBoot _sut;
        private readonly ITenantIdProvider _tenantIdProvider;

        public TheGenerateDataOnBootDecorator()
        {
            _compositionRoot = A.Fake<ICompositionRoot>();
            _dataGenerationModule = A.Fake<IModule>();
            _dataGenerationContext = A.Fake<IDataGenerationContext>();
            _tenantIdProvider = A.Fake<ITenantIdProvider>();

            var backendFxApplication = A.Fake<IBackendFxApplication>();
            A.CallTo(() => backendFxApplication.CompositionRoot).Returns(_compositionRoot);
            _sut = new GenerateDataOnBoot(
                _tenantIdProvider,
                _dataGenerationModule,
                backendFxApplication);

            _sut.SetPrivate(f => f.DataGenerationContext, _dataGenerationContext);
        }

        [Fact]
        public void DelegatesAllOtherCalls()
        {
            var app = A.Fake<IBackendFxApplication>();
            IBackendFxApplication sut = new GenerateDataOnBoot(A.Fake<ITenantIdProvider>(), A.Fake<IModule>(), app);


            // ReSharper disable UnusedVariable
            var ai = sut.AsyncInvoker;
            A.CallTo(() => app.AsyncInvoker).MustHaveHappenedOnceExactly();

            var cr = sut.CompositionRoot;
            A.CallTo(() => app.CompositionRoot).MustHaveHappenedOnceOrMore();

            sut.Dispose();
            A.CallTo(() => app.Dispose()).MustHaveHappenedOnceExactly();

            var i = sut.Invoker;
            A.CallTo(() => app.Invoker).MustHaveHappenedOnceOrMore();

            var mb = sut.MessageBus;
            A.CallTo(() => app.MessageBus).MustHaveHappenedOnceExactly();

            // ReSharper restore UnusedVariable
        }

        [Fact]
        public async Task RegistersDataGenerationModuleOnBoot()
        {
            await _sut.BootAsync();
            A.CallTo(() => _compositionRoot.RegisterModules(A<IModule[]>.That.Contains(_dataGenerationModule)))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task RunsDataGeneratorsOnBoot()
        {
            var tenantId1 = new TenantId(1);
            var tenantId2 = new TenantId(2);
            var tenantId3 = new TenantId(3);
            var tenantId4 = new TenantId(4);

            A.CallTo(() => _tenantIdProvider.GetActiveDemonstrationTenantIds()).Returns(new[] { tenantId1, tenantId2 });
            A.CallTo(() => _tenantIdProvider.GetActiveProductionTenantIds()).Returns(new[] { tenantId3, tenantId4 });
            await _sut.BootAsync();
            A.CallTo(
                    () => _dataGenerationContext.SeedDataForTenant(
                        A<TenantId>.That.IsEqualTo(tenantId1),
                        A<bool>.That.IsEqualTo(true)))
                .MustHaveHappenedOnceExactly();
            A.CallTo(
                    () => _dataGenerationContext.SeedDataForTenant(
                        A<TenantId>.That.IsEqualTo(tenantId2),
                        A<bool>.That.IsEqualTo(true)))
                .MustHaveHappenedOnceExactly();
            A.CallTo(
                    () => _dataGenerationContext.SeedDataForTenant(
                        A<TenantId>.That.IsEqualTo(tenantId3),
                        A<bool>.That.IsEqualTo(false)))
                .MustHaveHappenedOnceExactly();
            A.CallTo(
                    () => _dataGenerationContext.SeedDataForTenant(
                        A<TenantId>.That.IsEqualTo(tenantId4),
                        A<bool>.That.IsEqualTo(false)))
                .MustHaveHappenedOnceExactly();

            Assert.True(_sut.WaitForBoot(1000));
        }
    }
}
