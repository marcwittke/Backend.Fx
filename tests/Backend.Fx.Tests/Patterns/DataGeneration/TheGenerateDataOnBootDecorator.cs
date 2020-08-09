using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Hacking;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DataGeneration
{
    public class TheGenerateDataOnBootDecorator
    {
        public TheGenerateDataOnBootDecorator()
        {
            _compositionRoot = A.Fake<ICompositionRoot>();
            _dataGenerationModule = A.Fake<IModule>();
            _dataGenerationContext = A.Fake<IDataGenerationContext>();
            var backendFxApplication = A.Fake<IBackendFxApplication>();
            A.CallTo(() => backendFxApplication.CompositionRoot).Returns(_compositionRoot);
            _sut = new GenerateDataOnBoot(A.Fake<ITenantService>(),
                                          _dataGenerationModule, backendFxApplication);

            _sut.SetPrivate(f => f.DataGenerationContext, _dataGenerationContext);
        }

        private readonly GenerateDataOnBoot _sut;
        private readonly IModule _dataGenerationModule;
        private readonly IDataGenerationContext _dataGenerationContext;
        private readonly ICompositionRoot _compositionRoot;

        [Fact]
        public void DelegatesAllOtherCalls()
        {
            var app = A.Fake<IBackendFxApplication>();
            IBackendFxApplication sut = new GenerateDataOnBoot(A.Fake<ITenantService>(), A.Fake<IModule>(), app);


            // ReSharper disable UnusedVariable
            IBackendFxApplicationAsyncInvoker ai = sut.AsyncInvoker;
            A.CallTo(() => app.AsyncInvoker).MustHaveHappenedOnceExactly();

            ICompositionRoot cr = sut.CompositionRoot;
            A.CallTo(() => app.CompositionRoot).MustHaveHappenedOnceOrMore();

            sut.Dispose();
            A.CallTo(() => app.Dispose()).MustHaveHappenedOnceExactly();

            IBackendFxApplicationInvoker i = sut.Invoker;
            A.CallTo(() => app.Invoker).MustHaveHappenedOnceOrMore();

            IMessageBus mb = sut.MessageBus;
            A.CallTo(() => app.MessageBus).MustHaveHappenedOnceExactly();

            var b = sut.WaitForBoot();
            A.CallTo(() => app.WaitForBoot(A<int>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            // ReSharper restore UnusedVariable
        }

        [Fact]
        public async Task RegistersDataGenerationModuleOnBoot()
        {
            await _sut.Boot();
            A.CallTo(() => _compositionRoot.RegisterModules(A<IModule[]>.That.Contains(_dataGenerationModule))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task RunsDataGeneratorsOnBoot()
        {
            await _sut.Boot();
            A.CallTo(() => _dataGenerationContext.SeedDataForAllActiveTenants()).MustHaveHappenedOnceExactly();
        }
    }
}