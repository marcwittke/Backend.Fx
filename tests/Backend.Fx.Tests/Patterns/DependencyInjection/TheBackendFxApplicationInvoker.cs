using System;
using System.Security.Principal;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheBackendFxApplicationInvoker
    {
        private readonly IBackendFxApplicationInvoker _sut;
        private readonly DITestFakes _fakes;

        public TheBackendFxApplicationInvoker()
        {
            _fakes = new DITestFakes();
            _sut = new BackendFxApplicationInvoker(_fakes.CompositionRoot, _fakes.ExceptionLogger);
        }

        [Fact]
        public void BeginsNewScopeForEveryInvocation()
        {
            _sut.Invoke(ip=>{}, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() =>_fakes.CompositionRoot.BeginScope()).MustHaveHappenedOnceExactly();
            A.CallTo(() =>_fakes.InjectionScope.Dispose()).MustHaveHappenedOnceExactly();
            
            _sut.Invoke(ip=>{}, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() =>_fakes.CompositionRoot.BeginScope()).MustHaveHappenedTwiceExactly();
            A.CallTo(() =>_fakes.InjectionScope.Dispose()).MustHaveHappenedTwiceExactly();
        }
        
        [Fact]
        public void BeginsNewUnitOfWorkForEveryInvocation()
        {
            _sut.Invoke(ip=>{}, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.UnitOfWork.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.UnitOfWork.Complete()).MustHaveHappenedOnceExactly();
            
            _sut.Invoke(ip=>{}, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.UnitOfWork.Begin()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => _fakes.UnitOfWork.Complete()).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public void MaintainsTenantIdOnInvocation()
        {
            var tenantId = new TenantId(123);
            _sut.Invoke(ip => {}, new AnonymousIdentity(), tenantId);
            A.CallTo(() => _fakes.TenantIdHolder.ReplaceCurrent(A<TenantId>.That.IsEqualTo(tenantId))).MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public void MaintainsIdentityOnInvocation()
        {
            var identity = new GenericIdentity("me");
            _sut.Invoke(ip => {}, identity, new TenantId(123));
            A.CallTo(() => _fakes.IdentityHolder.ReplaceCurrent(A<IIdentity>.That.IsEqualTo(identity))).MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public void MaintainsCorrelationIdOnInvocation()
        {
            var correlationId = Guid.NewGuid();
            _sut.Invoke(ip => {}, new AnonymousIdentity(), new TenantId(123), correlationId);
            Assert.Equal(correlationId, _fakes.CurrentCorrelationHolder.Current.Id);
        }
        
        [Fact]
        public void DoesNotCompleteUnitOfWorkOnException()
        {
            _sut.Invoke(ip=> throw new InvalidOperationException(), new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.UnitOfWork.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.UnitOfWork.Complete()).MustNotHaveHappened();
        }
        
        [Fact]
        public void LogsException()
        {
            _sut.Invoke(ip=> throw new InvalidOperationException(), new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.ExceptionLogger.LogException(A<InvalidOperationException>._)).MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public void ProvidesInstanceProviderForInvocation()
        {
            IInstanceProvider provided = null;
            _sut.Invoke(ip=> provided = ip, new AnonymousIdentity(), new TenantId(111));
            Assert.StrictEqual(_fakes.InstanceProvider, provided);
        }
    }
}