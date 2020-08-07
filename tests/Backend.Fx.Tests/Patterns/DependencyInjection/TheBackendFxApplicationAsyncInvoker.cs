using System;
using System.Reflection;
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
        private readonly IBackendFxApplicationAsyncInvoker _sut;
        private readonly DITestFakes _fakes;

        public TheBackendFxApplicationAsyncInvoker()
        {
            _fakes = new DITestFakes();
            _sut = new BackendFxApplicationInvoker(_fakes.CompositionRoot, _fakes.ExceptionLogger);
        }

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
        public async Task CatchesFrameworkExceptions()
        {
            A.CallTo(() => _fakes.CompositionRoot.BeginScope()).Throws<InvalidOperationException>();
            await _sut.InvokeAsync(ip=> Task.CompletedTask, new AnonymousIdentity(), new TenantId(111));
        }
        
        [Fact]
        public async Task BeginsNewUnitOfWorkForEveryInvocation()
        {
            await _sut.InvokeAsync(ip => Task.CompletedTask, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.UnitOfWork.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.UnitOfWork.Complete()).MustHaveHappenedOnceExactly();

            await _sut.InvokeAsync(ip => Task.CompletedTask, new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.UnitOfWork.Begin()).MustHaveHappenedTwiceExactly();
            A.CallTo(() => _fakes.UnitOfWork.Complete()).MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task MaintainsTenantIdOnInvocation()
        {
            var tenantId = new TenantId(123);
            await _sut.InvokeAsync(ip => Task.CompletedTask, new AnonymousIdentity(), tenantId);
            A.CallTo(() => _fakes.TenantIdHolder.ReplaceCurrent(A<TenantId>.That.IsEqualTo(tenantId))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task MaintainsIdentityOnInvocation()
        {
            var identity = new GenericIdentity("me");
            await _sut.InvokeAsync(ip => Task.CompletedTask, identity, new TenantId(123));
            A.CallTo(() => _fakes.IdentityHolder.ReplaceCurrent(A<IIdentity>.That.IsEqualTo(identity))).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task MaintainsCorrelationIdOnInvocation()
        {
            var correlationId = Guid.NewGuid();
            await _sut.InvokeAsync(ip => Task.CompletedTask, new AnonymousIdentity(), new TenantId(123), correlationId);
            Assert.Equal(correlationId, _fakes.CurrentCorrelationHolder.Current.Id);
        }

        [Fact]
        public async Task DoesNotCompleteUnitOfWorkOnException()
        {
            await _sut.InvokeAsync(ip => throw new InvalidOperationException(), new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.UnitOfWork.Begin()).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakes.UnitOfWork.Complete()).MustNotHaveHappened();
        }

        [Fact]
        public async Task LogsException()
        {
            await _sut.InvokeAsync(ip => throw new InvalidOperationException(), new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.ExceptionLogger.LogException(A<InvalidOperationException>._)).MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public async Task LogsTargetInvocationExceptionUnwrapped()
        {
            await _sut.InvokeAsync(ip=> throw new TargetInvocationException(new InvalidOperationException()), new AnonymousIdentity(), new TenantId(111));
            A.CallTo(() => _fakes.ExceptionLogger.LogException(A<InvalidOperationException>._)).MustHaveHappenedOnceExactly();
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