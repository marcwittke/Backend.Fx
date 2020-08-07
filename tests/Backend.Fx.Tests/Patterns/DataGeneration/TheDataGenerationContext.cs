using System;
using System.Linq;
using System.Security.Principal;
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
        private readonly DataGenerationContext _sut;
        private readonly IDemoDataGenerator[] _demoDataGenerators = {new DemoDataGenerator1(), new DemoDataGenerator2()};
        private readonly IProductiveDataGenerator[] _prodDataGenerators = {new ProdDataGenerator1()};
        private readonly TenantId[] _demoTenants = {new TenantId(1), new TenantId(2)};
        private readonly TenantId[] _prodTenants = {new TenantId(11), new TenantId(12)};
        private DITestFakes _fakes;

        public TheDataGenerationContext()
        {
            _fakes = new DITestFakes();
            A.CallTo(() => _fakes.InstanceProvider.GetInstances<IDataGenerator>()).Returns(_demoDataGenerators.Concat(_prodDataGenerators.Cast<IDataGenerator>()).ToArray());

            var messageBus = new InMemoryMessageBus();
            messageBus.ProvideInvoker(_fakes.Invoker);
            var tenantService = A.Fake<ITenantService>();
            A.CallTo(() => tenantService.GetActiveDemonstrationTenantIds()).Returns(_demoTenants);
            A.CallTo(() => tenantService.GetActiveProductionTenantIds()).Returns(_prodTenants);
            A.CallTo(() => tenantService.GetActiveTenantIds()).Returns(_prodTenants.Concat(_demoTenants).ToArray());
            _sut = new DataGenerationContext(
                tenantService,
                _fakes.CompositionRoot,
                _fakes.Invoker);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CallsDataGeneratorWhenSeedingForSpecificTenant(bool isDemoTenant)
        {
            _sut.SeedDataForTenant(new TenantId(123), isDemoTenant);

            foreach (IProductiveDataGenerator dataGenerator in _prodDataGenerators)
            {
                A.CallTo(() => ((ProdDataGenerator) dataGenerator).Impl.Generate()).MustHaveHappenedOnceExactly();
            }

            foreach (IDemoDataGenerator dataGenerator in _demoDataGenerators)
            {
                if (isDemoTenant)
                {
                    A.CallTo(() => ((DemoDataGenerator) dataGenerator).Impl.Generate()).MustHaveHappenedOnceExactly();
                }
                else
                {
                    A.CallTo(() => ((DemoDataGenerator) dataGenerator).Impl.Generate()).MustNotHaveHappened();
                }
            }
        }

        [Fact]
        public void CallsDataGeneratorWhenSeedingForAllTenants()
        {
            _sut.SeedDataForAllActiveTenants();

            var tenantIds = _demoTenants.Concat(_prodTenants).ToArray();
            foreach (TenantId tenantId in tenantIds)
            {
                int expectedScopeCount = _prodDataGenerators.Length;
                if (_demoTenants.Contains(tenantId))
                {
                    expectedScopeCount += _demoDataGenerators.Length;
                }
                A.CallTo(() => _fakes.Invoker.Invoke(A<Action<IInstanceProvider>>._, A<IIdentity>._, A<TenantId>.That.IsSameAs(tenantId), A<Guid?>._))
                 .MustHaveHappenedANumberOfTimesMatching(i => i == expectedScopeCount);
                
                foreach (IProductiveDataGenerator dataGenerator in _prodDataGenerators)
                {
                    A.CallTo(() => ((ProdDataGenerator) dataGenerator).Impl.Generate()).MustHaveHappenedANumberOfTimesMatching(i => i == tenantIds.Length);
                }
                
                foreach (IDemoDataGenerator dataGenerator in _demoDataGenerators)
                {
                    A.CallTo(() => ((DemoDataGenerator) dataGenerator).Impl.Generate()).MustHaveHappenedANumberOfTimesMatching(i => i == _demoTenants.Length);
                }
            }
        }

        private abstract class DemoDataGenerator : IDemoDataGenerator
        {
            public readonly IDemoDataGenerator Impl;

            protected DemoDataGenerator()
            {
                Impl= A.Fake<IDemoDataGenerator>(o => o.Named(GetType().Name));
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
                Impl= A.Fake<IProductiveDataGenerator>(o => o.Named(GetType().Name));
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