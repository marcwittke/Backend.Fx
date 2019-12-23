using System;
using System.Collections.Generic;
using System.Threading;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Extensions;
using Xunit;

namespace Backend.Fx.Tests.Environment.DateAndTime
{
    public class TheWallClock
    {
        private readonly IEqualityComparer<DateTime?> _tolerantDateTimeComparer = new TolerantDateTimeComparer(TimeSpan.FromMilliseconds(10));

        [Fact]
        public void IsTheSystemClock()
        {
            IClock sut = new WallClock();
            
            Assert.Equal(DateTime.UtcNow, sut.UtcNow, _tolerantDateTimeComparer);

            Thread.Sleep(100);

            Assert.Equal(DateTime.UtcNow, sut.UtcNow, _tolerantDateTimeComparer);
        }
    }
}
