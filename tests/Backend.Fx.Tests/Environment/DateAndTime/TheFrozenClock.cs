using System.Threading;
using Backend.Fx.ExecutionPipeline;
using Backend.Fx.TestUtil;
using NodaTime;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Environment.DateAndTime
{
    public class TheFrozenClock : TestWithLogging
    {
        [Fact]
        public void IsFrozen()
        {
            
            IClock sut = new FrozenClock(SystemClock.Instance);
            Instant systemUtcNow = sut.GetCurrentInstant();
            Thread.Sleep(100);
            Assert.Equal(systemUtcNow, sut.GetCurrentInstant());
            Assert.NotEqual(SystemClock.Instance.GetCurrentInstant(), sut.GetCurrentInstant());
        }

        public TheFrozenClock(ITestOutputHelper output) : base(output)
        {
        }
    }
}