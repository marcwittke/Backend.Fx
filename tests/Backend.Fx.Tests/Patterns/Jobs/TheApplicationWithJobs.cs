using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Backend.Fx.Patterns.Jobs;
using Backend.Fx.Tests.Patterns.DependencyInjection;
using FakeItEasy;
using FluentScheduler;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using IJob = Backend.Fx.Patterns.Jobs.IJob;

namespace Backend.Fx.Tests.Patterns.Jobs
{
    public class TheApplicationWithJobs : TestWithLogging
    {
        private readonly ITenantIdProvider _tenantIdProvider = A.Fake<ITenantIdProvider>();

        public TheApplicationWithJobs(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            SayHelloJob.Invocations.Clear();
            A.CallTo(() => _tenantIdProvider.GetActiveTenantIds()).ReturnsLazily(() => new[]
            {
                new TenantId(123),
                new TenantId(234),
                new TenantId(345),
                new TenantId(456),
            });
        }

        [Theory]
        [InlineData(CompositionRootType.Microsoft)]
        [InlineData(CompositionRootType.SimpleInjector)]
        public async Task RunsJobs(CompositionRootType compositionRootType)
        {
            using var sut = new FluentSchedulerApplication(
                _tenantIdProvider,
                new BackendFxApplication(
                    compositionRootType.Create(),
                    new ExceptionLoggers(),
                    typeof(TheApplicationWithJobs).Assembly));

            await sut.BootAsync();

            await Task.Delay(TimeSpan.FromSeconds(1));

            sut.Dispose();

            Assert.True(SayHelloJob.Invocations.Count > 35);
            Assert.True(SayHelloJob.Invocations.Count < 45);
        }

        private class FluentSchedulerApplication : ApplicationWithJobs
        {
            private readonly ITenantIdProvider _tenantIdProvider;

            public FluentSchedulerApplication(ITenantIdProvider tenantIdProvider, IBackendFxApplication application)
                : base(application)
            {
                _tenantIdProvider = tenantIdProvider;
                application.CompositionRoot.RegisterModules(new JobModule(Assemblies));
            }

            public override async Task BootAsync(CancellationToken cancellationToken = default)
            {
                await base.BootAsync(cancellationToken);
                JobManager.RemoveAllJobs();
                JobManager.Initialize(new JobSchedule(this, _tenantIdProvider));
            }
        }

        private class JobSchedule : Registry
        {
            private readonly AllTenantBackendFxApplicationInvoker _invoker;

            public JobSchedule(IBackendFxApplication application, ITenantIdProvider tenantIdProvider)
            {
                _invoker = new AllTenantBackendFxApplicationInvoker(tenantIdProvider, application.Invoker);
                NonReentrantAsDefault();

                Schedule(() => _invoker.Invoke(sp => sp.GetRequiredService<SayHelloJob>().Run()))
                    .ToRunNow()
                    .AndEvery(interval: 111)
                    .Milliseconds();
            }
        }
    }

    public class SayHelloJob : IJob
    {
        private readonly ICurrentTHolder<TenantId> _tenantIdHolder;
        public static List<string> Invocations = new List<string>();

        public SayHelloJob(ICurrentTHolder<TenantId> tenantIdHolder)
        {
            _tenantIdHolder = tenantIdHolder;
        }

        public void Run()
        {
            Invocations.Add($"Hello Tenant {_tenantIdHolder.Current}");
        }
    }
}