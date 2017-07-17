namespace Backend.Fx.Tests.Environment.DateAndTime
{
    using System;
    using System.Threading;
    using Fx.Environment.DateAndTime;
    using Xunit;

    public class TheClock
    {
        [Fact]
        public void AllowsOverridingOfUtcNow()
        {
            var overriddenUtcNow = new DateTime(2000, 1, 1, 12, 0, 0);
            Clock sut = new WallClock();
            sut.OverrideUtcNow(overriddenUtcNow);

            Assert.Equal(overriddenUtcNow, sut.UtcNow);
            Thread.Sleep(100);
            Assert.Equal(overriddenUtcNow, sut.UtcNow);
        }
    }
}