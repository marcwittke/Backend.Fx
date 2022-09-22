using System;
using System.Security.Principal;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.TestUtil;
using FakeItEasy;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheBackendFxApplicationInvoker : TestWithLogging
    {
        public TheBackendFxApplicationInvoker(ITestOutputHelper output): base(output)
        {
            _fakes = new DiTestFakes();
            _sut = new BackendFxApplicationInvoker(_fakes.CompositionRoot);
        }

        private readonly IBackendFxApplicationInvoker _sut;
        private readonly DiTestFakes _fakes;

        [Fact]
        public void BeginsNewScopeForEveryInvocation()
        {
            _sut.Invoke(_ => { }, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.CompositionRoot.BeginScope()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.ServiceScope.Dispose()).MustHaveHappenedOnceExactly();

            _sut.Invoke(_ => { }, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.CompositionRoot.BeginScope()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => _fakes.ServiceScope.Dispose()).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public void BeginsNewOperationForEveryInvocation()
        {
            _sut.Invoke(_ => { }, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.Operation.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.Operation.Complete()).MustHaveHappenedOnceExactly();

            _sut.Invoke(_ => { }, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.Operation.Begin()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => _fakes.Operation.Complete()).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public void DoesNotCatchFrameworkExceptions()
        {
            A.CallTo(() => _fakes.CompositionRoot.BeginScope()).Throws<SimulatedException>();
            Assert.Throws<SimulatedException>(() => _sut.Invoke(_ => { }, new AnonymousIdentity(), new TenantId(111)));
            A.CallTo(() => _fakes.Operation.Begin()).MustNotHaveHappened();
            A.CallTo(() => _fakes.Operation.Complete()).MustNotHaveHappened();
        }

        [Fact]
        public void DoesNotCatchOperationExceptions()
        {
            Assert.Throws<SimulatedException>(() => _sut.Invoke(_ => throw new SimulatedException(), new AnonymousIdentity(), new TenantId(111)));
            A.CallTo(() => _fakes.Operation.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.Operation.Complete()).MustNotHaveHappened();
        }

        [Fact]
        public void MaintainsCorrelationIdOnInvocation()
        {
            var correlationId = Guid.NewGuid();
            _sut.Invoke(_ => { }, new AnonymousIdentity(), new TenantId(123), correlationId);
            Assert.Equal(correlationId, _fakes.CurrentCorrelationHolder.Current.Id);
        }

        [Fact]
        public void MaintainsIdentityOnInvocation()
        {
            var identity = new GenericIdentity("me");
            _sut.Invoke(_ => { }, identity, new TenantId(123));
            A.CallTo(() => _fakes.IdentityHolder.ReplaceCurrent(A<IIdentity>.That.IsEqualTo(identity))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void MaintainsTenantIdOnInvocation()
        {
            var tenantId = new TenantId(123);
            _sut.Invoke(_ => { }, new AnonymousIdentity(), tenantId);
            A.CallTo(() => _fakes.TenantIdHolder.ReplaceCurrent(A<TenantId>.That.IsEqualTo(tenantId))).MustHaveHappenedOnceExactly();
        }


        [Fact]
        public void ProvidesServiceProviderForInvocation()
        {
            IServiceProvider provided = null;
            _sut.Invoke(sp => provided = sp, new AnonymousIdentity(), new TenantId(111));
            Assert.StrictEqual(_fakes.ServiceProvider, provided);
        }
    }
}