using System;
using System.Reflection;
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
        public TheBackendFxApplicationInvoker()
        {
            _fakes = new DiTestFakes();
            _sut = new BackendFxApplicationInvoker(_fakes.CompositionRoot, _fakes.ExceptionLogger);
        }

        private readonly IBackendFxApplicationInvoker _sut;
        private readonly DiTestFakes _fakes;

        [Fact]
        public void BeginsNewScopeForEveryInvocation()
        {
            _sut.Invoke(ip => { }, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.CompositionRoot.BeginScope()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.InjectionScope.Dispose()).MustHaveHappenedOnceExactly();

            _sut.Invoke(ip => { }, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.CompositionRoot.BeginScope()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => _fakes.InjectionScope.Dispose()).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public void BeginsNewOperationForEveryInvocation()
        {
            _sut.Invoke(ip => { }, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.Operation.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.Operation.Complete()).MustHaveHappenedOnceExactly();

            _sut.Invoke(ip => { }, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.Operation.Begin()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => _fakes.Operation.Complete()).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public void CatchesFrameworkExceptions()
        {
            A.CallTo(() => _fakes.CompositionRoot.BeginScope()).Throws<InvalidOperationException>();
            _sut.Invoke(ip => { }, new AnonymousIdentity(), new TenantId(111));
        }

        [Fact]
        public void DoesNotCompleteOperationOnException()
        {
            _sut.Invoke(ip => throw new InvalidOperationException(), new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.Operation.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.Operation.Complete()).MustNotHaveHappened();
        }

        [Fact]
        public void LogsException()
        {
            _sut.Invoke(ip => throw new InvalidOperationException(), new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.ExceptionLogger.LogException(A<InvalidOperationException>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void LogsTargetInvocationExceptionUnwrapped()
        {
            _sut.Invoke(ip => throw new TargetInvocationException(new InvalidOperationException()), new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.ExceptionLogger.LogException(A<InvalidOperationException>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void MaintainsCorrelationIdOnInvocation()
        {
            var correlationId = Guid.NewGuid();
            _sut.Invoke(ip => { }, new AnonymousIdentity(), new TenantId(123), correlationId);
            Assert.Equal(correlationId, _fakes.CurrentCorrelationHolder.Current.Id);
        }

        [Fact]
        public void MaintainsIdentityOnInvocation()
        {
            var identity = new GenericIdentity("me");
            _sut.Invoke(ip => { }, identity, new TenantId(123));
            A.CallTo(() => _fakes.IdentityHolder.ReplaceCurrent(A<IIdentity>.That.IsEqualTo(identity))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void MaintainsTenantIdOnInvocation()
        {
            var tenantId = new TenantId(123);
            _sut.Invoke(ip => { }, new AnonymousIdentity(), tenantId);
            A.CallTo(() => _fakes.TenantIdHolder.ReplaceCurrent(A<TenantId>.That.IsEqualTo(tenantId))).MustHaveHappenedOnceExactly();
        }


        [Fact]
        public void ProvidesInstanceProviderForInvocation()
        {
            IInstanceProvider provided = null;
            _sut.Invoke(ip => provided = ip, new AnonymousIdentity(), new TenantId(111));
            Assert.StrictEqual(_fakes.InstanceProvider, provided);
        }
    }
}