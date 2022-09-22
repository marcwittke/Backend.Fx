using System;
using System.Threading;
using Backend.Fx.Hacking;
using Backend.Fx.TestUtil;
using NodaTime;
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
            var overriddenUtcNow = Instant.FromUtc(2000, 1, 1, 12, 0, 0);
            var sut = new AdjustableClock(SystemClock.Instance);
            sut.OverrideUtcNow(overriddenUtcNow);
            Assert.Equal(overriddenUtcNow, sut.GetCurrentInstant());
            Thread.Sleep(100);
            Assert.Equal(overriddenUtcNow, sut.GetCurrentInstant());
        }

        public TheAdjustableClock(ITestOutputHelper output) : base(output)
        {
        }
    }
}