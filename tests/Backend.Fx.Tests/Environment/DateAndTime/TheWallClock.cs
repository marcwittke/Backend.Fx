namespace Backend.Fx.Tests.Environment.DateAndTime
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Fx.Environment.DateAndTime;
    using NLogLogging;
    using Testing;
    using Xunit;

    public class TheWallClock : IClassFixture<NLogLoggingFixture>
    {
        private readonly IEqualityComparer<DateTime?> tolerantDateTimeComparer = new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(10));

        [Fact]
        public void IsTheSystemClock()
        {
            IClock sut = new WallClock();
            
            Assert.Equal(DateTime.UtcNow, sut.UtcNow, tolerantDateTimeComparer);

            Thread.Sleep(100);

            Assert.Equal(DateTime.UtcNow, sut.UtcNow, tolerantDateTimeComparer);
        }
    }
}
