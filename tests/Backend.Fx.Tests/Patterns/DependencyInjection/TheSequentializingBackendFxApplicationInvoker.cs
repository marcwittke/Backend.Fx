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

        private readonly int _actionDuration = 100;
        private readonly IBackendFxApplicationInvoker _decoratedInvoker;
        private readonly IBackendFxApplicationInvoker _invoker;

        public TheSequentializingBackendFxApplicationInvoker()
        {
            var fakes = new DiTestFakes();
            _invoker = new BackendFxApplicationInvoker(fakes.CompositionRoot);
            _decoratedInvoker = new SequentializingBackendFxApplicationInvoker(_invoker);
        }

        private async Task InvokeSomeActions(int count, IBackendFxApplicationInvoker invoker)
        {
            Task[] tasks = Enumerable
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
            if (System.Environment.ProcessorCount > 2)
            {
                // negative test: without sequentialization all tasks run in parallel
                var count = 10;
                var sw = new Stopwatch();
                sw.Start();
                await InvokeSomeActions(count, _invoker);
                long actualDuration = sw.ElapsedMilliseconds;
                int expectedDuration = _actionDuration * count;
                Assert.True(
                    actualDuration < expectedDuration,
                    $"Actual duration of {actualDuration}ms is greater than maximum expected duration of {expectedDuration}ms");
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
