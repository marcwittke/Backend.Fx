using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Logging;
using Backend.Fx.Patterns.DependencyInjection;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheSequentializingBackendFxApplicationInvoker
    {
        private static readonly ILogger Logger = LogManager.Create<TheSequentializingBackendFxApplicationInvoker>();
        
        public TheSequentializingBackendFxApplicationInvoker()
        {
            var fakes = new DiTestFakes();
            _invoker = new BackendFxApplicationInvoker(fakes.CompositionRoot);
            _decoratedInvoker = new SequentializingBackendFxApplicationInvoker(_invoker);
        }

        private readonly int _actionDuration = 100;
        private readonly IBackendFxApplicationInvoker _invoker;
        private readonly IBackendFxApplicationInvoker _decoratedInvoker;

        private async Task InvokeSomeActions(int count, IBackendFxApplicationInvoker invoker)
        {
            var tasks = Enumerable
                        .Range(0, count)
                        .Select(i => Task.Run(() => invoker.Invoke(DoTheAction, new AnonymousIdentity(), new TenantId(1))))
                        .ToArray();

            await Task.WhenAll(tasks);
        }

        private void DoTheAction(IInstanceProvider _)
        {
            Logger.Debug("start");
            Thread.Sleep(_actionDuration);
            Logger.Debug("end");
        }

        [Fact]
        public async Task IsReallyNeeded()
        {
            if (System.Environment.ProcessorCount > 1)
            {
                // negative test: without sequentialization all tasks run in parallel
                var count = 10;
                var sw = new Stopwatch();
                sw.Start();
                await InvokeSomeActions(count, _invoker);
                long actualDuration = sw.ElapsedMilliseconds;
                var expectedDuration = _actionDuration * count;
                Assert.True(actualDuration < expectedDuration,
                    $"Actual duration of {actualDuration}ms is greater than maximum expected duration of {expectedDuration}ms");
            }
            else
            {
                // fails on CI Pipeline due to CPU count   
            }
        }

        [Fact]
        public async Task SequentializesInvocations()
        {
            var count = 10;
            var sw = new Stopwatch();
            sw.Start();
            await InvokeSomeActions(count, _decoratedInvoker);
            Assert.True(sw.ElapsedMilliseconds >= _actionDuration * count);
        }
    }
}