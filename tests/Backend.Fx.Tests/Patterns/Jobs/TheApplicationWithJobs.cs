using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.Features.Jobs;
using Backend.Fx.Logging;
using Backend.Fx.TestUtil;
using Backend.Fx.Util;
using FakeItEasy;
using FluentScheduler;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using IJob = Backend.Fx.Features.Jobs.IJob;

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

            // now plus 3 x 300ms during a second, four tenants = 16 invocations
            Assert.True(SayHelloJob.Invocations.Count >= 12, $"Recorded {SayHelloJob.Invocations.Count} job invocations, while at least 12 were expected");
            
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
            public JobSchedule(IBackendFxApplication application, ITenantIdProvider tenantIdProvider)
            {
                var invoker = new AllTenantBackendFxApplicationInvoker(tenantIdProvider, application.Invoker);
                NonReentrantAsDefault();

                Schedule(() => invoker.Invoke(sp => sp.GetRequiredService<SayHelloJob>().Run()))
                    .ToRunNow()
                    .AndEvery(interval: 300)
                    .Milliseconds();
            }
        }
    }

    public class SayHelloJob : Features.Jobs.IJob
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