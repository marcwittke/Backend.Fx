using System;
using System.Threading;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Environment.DateAndTime
{
    public 
        class TheAdjustableClock : TestWithLogging
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

        public TheAdjustableClock(ITestOutputHelper output) : base(output)
        {
        }
    }
}