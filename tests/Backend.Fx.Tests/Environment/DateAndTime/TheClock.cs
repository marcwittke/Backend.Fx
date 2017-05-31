namespace Backend.Fx.Tests.Environment.DateAndTime
{
    using System;
    using System.Threading;
    using Fx.Environment.DateAndTime;
    using NLogLogging;
    using Xunit;

    public class TheClock : IClassFixture<NLogLoggingFixture>
    {
        private const int BuenosAiresUtcOffset = -180;
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

        [Fact]
        public void CanConvertToLocalTime()
        {
            var overriddenUtcNow = new DateTime(2000, 1, 1, 12, 0, 0);
            Clock sut = new WallClock();
            sut.OverrideUtcNow(overriddenUtcNow);
            var localNow = sut.GetLocalNow(BuenosAiresUtcOffset);
            Assert.Equal(new DateTime(2000, 1, 1, 9, 0, 0), localNow);
        }

        [Fact]
        public void CanLocalizeUtcMoment()
        {
            var moment = new DateTime(2000, 1, 1, 12, 0, 0);
            Clock sut = new WallClock();
            var localNow = sut.LocalizeUtcDateTime(moment, BuenosAiresUtcOffset);
            Assert.Equal(new DateTime(2000, 1, 1, 9, 0, 0), localNow);
        }

        [Fact]
        public void CanConvertLocalMomentToUtc()
        {
            var moment = new DateTime(2000, 1, 1, 12, 0, 0);
            Clock sut = new WallClock();
            var utcNow = sut.ToUtcTime(moment, BuenosAiresUtcOffset);
            Assert.Equal(new DateTime(2000, 1, 1, 15, 0, 0), utcNow);
        }
    }
}