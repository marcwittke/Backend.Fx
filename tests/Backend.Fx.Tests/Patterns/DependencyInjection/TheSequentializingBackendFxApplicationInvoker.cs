using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Environment.Authentication;
using Backend.Fx.Environment.MultiTenancy;
using Backend.Fx.Patterns.DependencyInjection;
using Xunit;

namespace Backend.Fx.Tests.Patterns.DependencyInjection
{
    public class TheSequentializingBackendFxApplicationInvoker
    {
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
                        .Select(i => Task.Run(() => invoker.Invoke(OneSecondAction, new AnonymousIdentity(), new TenantId(1))))
                        .ToArray();

            await Task.WhenAll(tasks);
        }

        private void OneSecondAction(IInstanceProvider _)
        {
            Console.WriteLine("start");
            Thread.Sleep(_actionDuration);
            Console.WriteLine("end");
        }

        [Fact]
        public async Task IsReallyNeeded()
        {
            // negative test: without sequentialization all tasks run in parallel
            var count = 10;
            var sw = new Stopwatch();
            sw.Start();
            await InvokeSomeActions(count, _invoker);
            Assert.True(sw.ElapsedMilliseconds < _actionDuration * count);
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