using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.DependencyInjection;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features;
using Backend.Fx.Features.DataGeneration;
using Backend.Fx.Features.DomainEvents;
using Backend.Fx.Features.DomainServices;
using Backend.Fx.Features.IdGeneration;
using Backend.Fx.Features.Jobs;
using Backend.Fx.Logging;
using Backend.Fx.MicrosoftDependencyInjection;
using Backend.Fx.SimpleInjectorDependencyInjection;
using Backend.Fx.Tests.DummyServices;
using Backend.Fx.Util;
using FakeItEasy;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests;

public abstract class TheBackendFxApplication : TestWithLogging
{
    private readonly IBackendFxApplication _sut;
    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();
    private readonly IEntityIdGenerator _entityIdGenerator = A.Fake<IEntityIdGenerator>();
    private readonly DummyServicesFeature _dummyServicesFeature = new();

    protected TheBackendFxApplication(ICompositionRoot compositionRoot, ITestOutputHelper output) : base(output)
    {
        _sut = new BackendFxApplication(compositionRoot, _exceptionLogger, GetType().Assembly);
        _sut.EnableFeature(new JobsFeature());
        _sut.EnableFeature(new DataGenerationFeature());
        _sut.EnableFeature(new DomainEventsFeature());
        _sut.EnableFeature(new DomainServicesFeature());
        _sut.EnableFeature(new IdGenerationFeature(_entityIdGenerator));
        _sut.EnableFeature(_dummyServicesFeature);
    }

    [Fact]
    public async Task DoesNotAllowEnablingFeaturesWhenBooted()
    {
        await _sut.BootAsync();
        Assert.Throws<InvalidOperationException>(() => _sut.EnableFeature(new DomainEventsFeature()));
    }

    [Fact]
    public async Task DoesNotSwallowExceptions()
    {
        await _sut.BootAsync();
        await Assert.ThrowsAsync<DivideByZeroException>(
            async () => await _sut.Invoker.InvokeAsync(
                _ => throw new DivideByZeroException(), new AnonymousIdentity()));
    }

    [Fact]
    public async Task LogsExceptions()
    {
        await _sut.BootAsync();
        try
        {
            await _sut.Invoker.InvokeAsync(_ => throw new DivideByZeroException(), new AnonymousIdentity());
        }
        catch (DivideByZeroException)
        {
            // expected
        }

        A.CallTo(() => _sut.ExceptionLogger.LogException(A<DivideByZeroException>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DoesNotWaitForBootWhenBooted()
    {
        await _sut.BootAsync();
        Assert.True(_sut.WaitForBoot(0));
    }

    [Fact]
    public void CanWaitForBoot()
    {
        const int delayMillisecondsOnBoot = 300;
        _sut.EnableFeature(new SlowBootingFeature(delayMillisecondsOnBoot));
        var booting = _sut.BootAsync();
        Assert.False(booting.IsCompleted);
        var sw = new Stopwatch();
        sw.Start();
        Assert.True(_sut.WaitForBoot((int)(delayMillisecondsOnBoot * 1.1)));
        Assert.True(booting.IsCompleted);
        Assert.True(sw.ElapsedMilliseconds >= delayMillisecondsOnBoot * 0.95);
    }

    [Fact]
    public async Task HasInjectedClock()
    {
        await _sut.BootAsync();
        await _sut.Invoker.InvokeAsync(sp =>
        {
            Assert.IsType<FrozenClock>(sp.GetRequiredService<IClock>());
            return Task.CompletedTask;
        }, new SystemIdentity());
    }

    [Fact]
    public async Task HasInjectedIdentityHolder()
    {
        await _sut.BootAsync();
        await _sut.Invoker.InvokeAsync(sp =>
        {
            var identityHolder = sp.GetRequiredService<ICurrentTHolder<IIdentity>>();
            Assert.IsType<CurrentIdentityHolder>(identityHolder);
            Assert.Equal(new SystemIdentity(), identityHolder.Current);
            return Task.CompletedTask;
        }, new SystemIdentity());
    }

    [Fact]
    public async Task HasInjectedCorrelationHolder()
    {
        await _sut.BootAsync();
        var firstCorrelationId = Guid.Empty;
        await _sut.Invoker.InvokeAsync(sp =>
        {
            var correlationHolder = sp.GetRequiredService<ICurrentTHolder<Correlation>>();
            Assert.IsType<CurrentCorrelationHolder>(correlationHolder);
            firstCorrelationId = correlationHolder.Current.Id;
            return Task.CompletedTask;
        }, new SystemIdentity());

        await _sut.Invoker.InvokeAsync(sp =>
        {
            var correlationHolder = sp.GetRequiredService<ICurrentTHolder<Correlation>>();
            Assert.IsType<CurrentCorrelationHolder>(correlationHolder);
            Assert.NotEqual(firstCorrelationId, correlationHolder.Current.Id);
            return Task.CompletedTask;
        }, new SystemIdentity());
    }

    [Fact]
    public async Task DisposesTheCompositionRoot()
    {
        var compositionRoot = A.Fake<ICompositionRoot>();

        using (var sut = new BackendFxApplication(compositionRoot, _exceptionLogger, GetType().Assembly))
        {
            sut.CompositionRoot.RegisterModules(new DummyServicesModule());
            await sut.BootAsync();
        }

        A.CallTo(() => compositionRoot.Dispose()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task WrapsInvocationsWithOperation()
    {
        await _sut.BootAsync();

        await _sut.Invoker.InvokeAsync(_ =>
        {
            A.CallTo(() => _dummyServicesFeature.Spies.OperationSpy.Begin(A<IServiceScope>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _dummyServicesFeature.Spies.OperationSpy.Complete()).MustNotHaveHappened();
            A.CallTo(() => _dummyServicesFeature.Spies.OperationSpy.Cancel()).MustNotHaveHappened();

            return Task.CompletedTask;
        });

        A.CallTo(() => _dummyServicesFeature.Spies.OperationSpy.Begin(A<IServiceScope>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _dummyServicesFeature.Spies.OperationSpy.Complete()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _dummyServicesFeature.Spies.OperationSpy.Cancel()).MustNotHaveHappened();
    }

    [Fact]
    public async Task DoesNotAllowToBeginOperationTwice()
    {
        await _sut.BootAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.Invoker.InvokeAsync(sp =>
        {
            sp.GetRequiredService<IOperation>().Begin(sp.CreateScope());
            return Task.CompletedTask;
        }));
    }
    
    [Fact]
    public async Task DoesNotAllowToCompleteOperationTwice()
    {
        await _sut.BootAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.Invoker.InvokeAsync(sp =>
        {
            sp.GetRequiredService<IOperation>().Complete();
            return Task.CompletedTask;
        }));
    }

    [Fact]
    public async Task CancelsOperationWhenInvocationFails()
    {
        await _sut.BootAsync();

        await Assert.ThrowsAsync<DivideByZeroException>(async () => await _sut.Invoker.InvokeAsync(_ =>
        {
            A.CallTo(() => _dummyServicesFeature.Spies.OperationSpy.Begin(A<IServiceScope>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _dummyServicesFeature.Spies.OperationSpy.Complete()).MustNotHaveHappened();
            A.CallTo(() => _dummyServicesFeature.Spies.OperationSpy.Cancel()).MustNotHaveHappened();

            throw new DivideByZeroException();
        }));

        A.CallTo(() => _dummyServicesFeature.Spies.OperationSpy.Begin(A<IServiceScope>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _dummyServicesFeature.Spies.OperationSpy.Complete()).MustNotHaveHappened();
        A.CallTo(() => _dummyServicesFeature.Spies.OperationSpy.Cancel()).MustHaveHappenedOnceExactly();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sut.Dispose();
        }
    }

    private class SlowBootingFeature : Feature, IBootableFeature
    {
        private readonly int _delayMillisecondsOnBoot;

        public SlowBootingFeature(int delayMillisecondsOnBoot)
        {
            _delayMillisecondsOnBoot = delayMillisecondsOnBoot;
        }

        public override void Enable(IBackendFxApplication application)
        {
        }

        public async Task BootAsync(IBackendFxApplication application, CancellationToken cancellationToken = default)
        {
            await Task.Delay(_delayMillisecondsOnBoot, cancellationToken);
        }
    }

    [UsedImplicitly]
    public class TheBackendFxApplicationWithMicrosoftDI : TheBackendFxApplication
    {
        public TheBackendFxApplicationWithMicrosoftDI(ITestOutputHelper output)
            : base(new MicrosoftCompositionRoot(), output)
        {
        }
    }

    [UsedImplicitly]
    public class TheBackendFxApplicationWithSimpleInjector : TheBackendFxApplication
    {
        public TheBackendFxApplicationWithSimpleInjector(ITestOutputHelper output)
            : base(new SimpleInjectorCompositionRoot(), output)
        {
        }
    }
}