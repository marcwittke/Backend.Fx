using System.Security.Principal;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Environment.Persistence;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.Patterns.UnitOfWork;
using Backend.Fx.RandomData;
using FakeItEasy;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class DITestFakes
    {
        private readonly int _sequenceNumber = TestRandom.Next(100000);
        
        public IUnitOfWork UnitOfWork { get; } = A.Fake<IUnitOfWork>();
        public ICurrentTHolder<TenantId> TenantIdHolder { get; } = A.Fake<ICurrentTHolder<TenantId>>();
        public ICurrentTHolder<IIdentity> IdentityHolder { get; } = A.Fake<ICurrentTHolder<IIdentity>>();
            
        public ICompositionRoot CompositionRoot { get; } = A.Fake<ICompositionRoot>();
        public CurrentCorrelationHolder CurrentCorrelationHolder { get; } = new CurrentCorrelationHolder();
        public IInjectionScope InjectionScope { get; } = A.Fake<IInjectionScope>();
        public IExceptionLogger ExceptionLogger { get; } = A.Fake<IExceptionLogger>();
        public IInstanceProvider InstanceProvider { get; } = A.Fake<IInstanceProvider>();
        public IMessageBus MessageBus { get; } = A.Fake<IMessageBus>();
        public IInfrastructureModule InfrastructureModule { get; } = A.Fake<IInfrastructureModule>();
        public IDatabaseBootstrapper DatabaseBootstrapper { get; } = A.Fake<IDatabaseBootstrapper>();

        public DITestFakes()
        {
            A.CallTo(() => InstanceProvider.GetInstance<ICurrentTHolder<Correlation>>()).Returns(CurrentCorrelationHolder);
            A.CallTo(() => InstanceProvider.GetInstance<ICurrentTHolder<TenantId>>()).Returns(TenantIdHolder);
            A.CallTo(() => InstanceProvider.GetInstance<ICurrentTHolder<IIdentity>>()).Returns(IdentityHolder);
            A.CallTo(() => InstanceProvider.GetInstance<IUnitOfWork>()).Returns(UnitOfWork);

            A.CallTo(() => InjectionScope.SequenceNumber).Returns(_sequenceNumber++);
            A.CallTo(() => InjectionScope.InstanceProvider).Returns(InstanceProvider);

            A.CallTo(() => CompositionRoot.BeginScope()).Returns(InjectionScope);
        }
    }
}