using System;
using System.Threading;
using Backend.Fx.Environment.DateAndTime;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Environment.DateAndTime
{
    public class TheFrozenClock : TestWithLogging
    {
        [Fact]
        public void IsFrozen()
        {

            IClock sut = new FrozenClock(new WallClock());
            DateTime systemUtcNow = sut.UtcNow;
            Thread.Sleep(100);
            Assert.Equal(systemUtcNow, sut.UtcNow);
            Assert.NotEqual(DateTime.UtcNow, sut.UtcNow);
        }

        [Fact]

        public void FrozenTimeIsKindUtc()
        {
            var sut = new FrozenClock(new WallClock());
            Assert.Equal(DateTimeKind.Utc, sut.UtcNow.Kind);
        }

        public TheFrozenClock(ITestOutputHelper output) : base(output)
        {
        }
    }
}