using System;
using System.Threading;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.TestUtil;
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

        public TheFrozenClock(ITestOutputHelper output) : base(output)
        {
        }
    }
}