using System;
using System.Threading;
using Backend.Fx.Environment.DateAndTime;
using Xunit;

namespace Backend.Fx.Tests.Environment.DateAndTime
{
    public class TheAdjustableClock
    {
        [Fact]
        public void AllowsOverridingOfUtcNow()
        {
            var overriddenUtcNow = new DateTime(2000, 1, 1, 12, 0, 0);
            var sut = new AdjustableClock(new WallClock());
            sut.OverrideUtcNow(overriddenUtcNow);
            Assert.Equal(overriddenUtcNow, sut.UtcNow);
            Thread.Sleep(100);
            Assert.Equal(overriddenUtcNow, sut.UtcNow);
        }
    }
}