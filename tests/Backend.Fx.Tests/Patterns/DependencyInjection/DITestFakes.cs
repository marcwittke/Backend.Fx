using System;
using System.Security.Principal;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.EventAggregation.Domain;
using Backend.Fx.Patterns.EventAggregation.Integration;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class DiTestFakes
    {
        public DiTestFakes()
        {
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.IsEqualTo(typeof(IDomainEventAggregator)))).Returns(DomainEventAggregator);
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.IsEqualTo(typeof(ICurrentTHolder<Correlation>)))).Returns(CurrentCorrelationHolder);
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.IsEqualTo(typeof(ICurrentTHolder<TenantId>)))).Returns(TenantIdHolder);
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.IsEqualTo(typeof(ICurrentTHolder<IIdentity>)))).Returns(IdentityHolder);
            A.CallTo(() => ServiceProvider.GetService(A<Type>.That.IsEqualTo(typeof(IOperation)))).Returns(Operation);
            

            A.CallTo(() => ServiceScope.ServiceProvider).Returns(ServiceProvider);

            A.CallTo(() => CompositionRoot.BeginScope()).Returns(ServiceScope);
        
            A.CallTo(() => Invoker.Invoke(A<Action<IServiceProvider>>._, A<IIdentity>._, A<TenantId>._, A<Guid?>._))
             .Invokes((Action<IServiceProvider> a, IIdentity i, TenantId t, Guid? g) => a.Invoke(ServiceProvider));
        }

        public ICurrentTHolder<TenantId> TenantIdHolder { get; } = A.Fake<ICurrentTHolder<TenantId>>();
        public ICurrentTHolder<IIdentity> IdentityHolder { get; } = A.Fake<ICurrentTHolder<IIdentity>>();
        public IOperation Operation { get; } = A.Fake<IOperation>();
        public IDomainEventAggregator DomainEventAggregator { get; } = A.Fake<IDomainEventAggregator>();
        public ICompositionRoot CompositionRoot { get; } = A.Fake<ICompositionRoot>();
        public CurrentCorrelationHolder CurrentCorrelationHolder { get; } = new CurrentCorrelationHolder();
        public IServiceScope ServiceScope { get; } = A.Fake<IServiceScope>();
        public IServiceProvider ServiceProvider { get; } = A.Fake<IServiceProvider>();
        public IBackendFxApplicationInvoker Invoker { get; } = A.Fake<IBackendFxApplicationInvoker>();
    }
}