using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using FakeItEasy;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheBackendFxApplicationAsyncInvoker
    {
        public TheBackendFxApplicationAsyncInvoker()
        {
            _fakes = new DiTestFakes();
            _sut = new BackendFxApplicationInvoker(_fakes.CompositionRoot);
        }

        private readonly IBackendFxApplicationAsyncInvoker _sut;
        private readonly DiTestFakes _fakes;

        [Fact]
        public async Task BeginsNewScopeForEveryInvocation()
        {
            await _sut.InvokeAsync(ip => Task.CompletedTask, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.CompositionRoot.BeginScope()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.InjectionScope.Dispose()).MustHaveHappenedOnceExactly();

            await _sut.InvokeAsync(ip => Task.CompletedTask, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.CompositionRoot.BeginScope()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => _fakes.InjectionScope.Dispose()).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task BeginsNewOperationForEveryInvocation()
        {
            await _sut.InvokeAsync(ip => Task.CompletedTask, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.Operation.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.Operation.Complete()).MustHaveHappenedOnceExactly();

            await _sut.InvokeAsync(ip => Task.CompletedTask, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.Operation.Begin()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => _fakes.Operation.Complete()).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task DoesNotCatchFrameworkExceptions()
        {
            A.CallTo(() => _fakes.CompositionRoot.BeginScope()).Throws<SimulatedException>();
            await Assert.ThrowsAsync<SimulatedException>(async () => await _sut.InvokeAsync(ip => Task.CompletedTask, new AnonymousIdentity(), new TenantId(111)));
            A.CallTo(() => _fakes.Operation.Begin()).MustNotHaveHappened();
            A.CallTo(() => _fakes.Operation.Complete()).MustNotHaveHappened();
        }

        [Fact]
        public async Task DoesNotCatchOperationExceptions()
        {
            await Assert.ThrowsAsync<SimulatedException>(async () => await _sut.InvokeAsync(ip => throw new SimulatedException(), new AnonymousIdentity(), new TenantId(111)));
            A.CallTo(() => _fakes.Operation.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.Operation.Complete()).MustNotHaveHappened();
        }

        [Fact]
        public async Task MaintainsCorrelationIdOnInvocation()
        {
            var correlationId = Guid.NewGuid();
            await _sut.InvokeAsync(ip => Task.CompletedTask, new AnonymousIdentity(), new TenantId(123), correlationId);
            Assert.Equal(correlationId, _fakes.CurrentCorrelationHolder.Current.Id);
        }

        [Fact]
        public async Task MaintainsIdentityOnInvocation()
        {
            var identity = new GenericIdentity("me");
            await _sut.InvokeAsync(ip => Task.CompletedTask, identity, new TenantId(123));
            A.CallTo(() => _fakes.IdentityHolder.ReplaceCurrent(A<IIdentity>.That.IsEqualTo(identity))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task MaintainsTenantIdOnInvocation()
        {
            var tenantId = new TenantId(123);
            await _sut.InvokeAsync(ip => Task.CompletedTask, new AnonymousIdentity(), tenantId);
            A.CallTo(() => _fakes.TenantIdHolder.ReplaceCurrent(A<TenantId>.That.IsEqualTo(tenantId))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ProvidesInstanceProviderForInvocation()
        {
            IInstanceProvider provided = null;
            await _sut.InvokeAsync(ip =>
            {
                provided = ip;
                return Task.CompletedTask;
            }, new AnonymousIdentity(), new TenantId(111));
            Assert.StrictEqual(_fakes.InstanceProvider, provided);
        }
    }
}