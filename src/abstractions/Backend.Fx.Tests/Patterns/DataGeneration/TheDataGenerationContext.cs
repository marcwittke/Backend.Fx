using System.Linq;
using System.Threading;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DataGeneration;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Tests.Patterns.DependencyInjection;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DataGeneration
{
    public class TheDataGenerationContext
    {
        public TheDataGenerationContext()
        {
            var fakes = new DiTestFakes();
            A.CallTo(() => fakes.InstanceProvider.GetInstances<IDataGenerator>()).Returns(_demoDataGenerators.Concat(_prodDataGenerators.Cast<IDataGenerator>()).ToArray());

            var application = A.Fake<IBackendFxApplication>();
            A.CallTo(() => application.Invoker).Returns(fakes.Invoker);
            A.CallTo(() => application.WaitForBoot(A<int>._, A<CancellationToken>._)).Returns(true);
            
            var messageBus = new InMemoryMessageBus();
            messageBus.ProvideInvoker(application.Invoker);
            
            var tenantIdProvider = A.Fake<ITenantIdProvider>();
            A.CallTo(() => tenantIdProvider.GetActiveDemonstrationTenantIds()).Returns(_demoTenants);
            A.CallTo(() => tenantIdProvider.GetActiveProductionTenantIds()).Returns(_prodTenants);
            
            _sut = new DataGenerationContext(fakes.CompositionRoot,
                                             fakes.Invoker);
        }

        private readonly DataGenerationContext _sut;
        private readonly IDemoDataGenerator[] _demoDataGenerators = {new DemoDataGenerator1(), new DemoDataGenerator2()};
        private readonly IProductiveDataGenerator[] _prodDataGenerators = {new ProdDataGenerator1()};
        private readonly TenantId[] _demoTenants = {new TenantId(1), new TenantId(2)};
        private readonly TenantId[] _prodTenants = {new TenantId(11), new TenantId(12)};

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CallsDataGeneratorWhenSeedingForSpecificTenant(bool isDemoTenant)
        {
            _sut.SeedDataForTenant(new TenantId(123), isDemoTenant);

            foreach (IProductiveDataGenerator dataGenerator in _prodDataGenerators)
                A.CallTo(() => ((ProdDataGenerator) dataGenerator).Impl.Generate()).MustHaveHappenedOnceExactly();

            foreach (IDemoDataGenerator dataGenerator in _demoDataGenerators)
                if (isDemoTenant)
                    A.CallTo(() => ((DemoDataGenerator) dataGenerator).Impl.Generate()).MustHaveHappenedOnceExactly();
                else
                    A.CallTo(() => ((DemoDataGenerator) dataGenerator).Impl.Generate()).MustNotHaveHappened();
        }

        private abstract class DemoDataGenerator : IDemoDataGenerator
        {
            public readonly IDemoDataGenerator Impl;

            protected DemoDataGenerator()
            {
                Impl = A.Fake<IDemoDataGenerator>(o => o.Named(GetType().Name));
            }

            public int Priority => Impl.Priority;

            public void Generate()
            {
                Impl.Generate();
            }
        }

        private class DemoDataGenerator1 : DemoDataGenerator
        {
        }

        private class DemoDataGenerator2 : DemoDataGenerator
        {
        }

        private abstract class ProdDataGenerator : IProductiveDataGenerator
        {
            public readonly IProductiveDataGenerator Impl;

            protected ProdDataGenerator()
            {
                Impl = A.Fake<IProductiveDataGenerator>(o => o.Named(GetType().Name));
            }

            public int Priority => Impl.Priority;

            public void Generate()
            {
                Impl.Generate();
            }
        }

        private class ProdDataGenerator1 : ProdDataGenerator
        {
        }
    }
}