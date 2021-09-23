using System;
using System.Security.Principal;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Integration;
using Backend.Fx.RandomData;
using FakeItEasy;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class DiTestFakes
    {
        private readonly int _sequenceNumber = TestRandom.Next(100000);

        public DiTestFakes()
        {
            A.CallTo(() => InstanceProvider.GetInstance<ICurrentTHolder<Correlation>>())
                .Returns(CurrentCorrelationHolder);
            A.CallTo(() => InstanceProvider.GetInstance<ICurrentTHolder<TenantId>>()).Returns(TenantIdHolder);
            A.CallTo(() => InstanceProvider.GetInstance<ICurrentTHolder<IIdentity>>()).Returns(IdentityHolder);
            A.CallTo(() => InstanceProvider.GetInstance<IOperation>()).Returns(Operation);

            A.CallTo(() => InjectionScope.SequenceNumber).Returns(_sequenceNumber++);
            A.CallTo(() => InjectionScope.InstanceProvider).Returns(InstanceProvider);

            A.CallTo(() => CompositionRoot.BeginScope()).Returns(InjectionScope);

            A.CallTo(() => Invoker.Invoke(A<Action<IInstanceProvider>>._, A<IIdentity>._, A<TenantId>._, A<Guid?>._))
                .Invokes((Action<IInstanceProvider> a, IIdentity _, TenantId _, Guid? _) => a.Invoke(InstanceProvider));
        }

        public ICurrentTHolder<TenantId> TenantIdHolder { get; } = A.Fake<ICurrentTHolder<TenantId>>();

        public ICurrentTHolder<IIdentity> IdentityHolder { get; } = A.Fake<ICurrentTHolder<IIdentity>>();

        public IOperation Operation { get; } = A.Fake<IOperation>();

        public ICompositionRoot CompositionRoot { get; } = A.Fake<ICompositionRoot>();

        public CurrentCorrelationHolder CurrentCorrelationHolder { get; } = new();

        public IInjectionScope InjectionScope { get; } = A.Fake<IInjectionScope>();

        public IExceptionLogger ExceptionLogger { get; } = A.Fake<IExceptionLogger>();

        public IInstanceProvider InstanceProvider { get; } = A.Fake<IInstanceProvider>();

        public IMessageBus MessageBus { get; } = A.Fake<IMessageBus>();

        public IBackendFxApplicationInvoker Invoker { get; } = A.Fake<IBackendFxApplicationInvoker>();
    }
}
