using System;
using System.Collections.Generic;
using System.Threading;
using Backend.Fx.Environment.DateAndTime;
using Backend.Fx.Extensions;
using Backend.Fx.TestUtil;
using Xunit;
using Xunit.Abstractions;

namespace Backend.Fx.Tests.Environment.DateAndTime
{
    public class TheWallClock : TestWithLogging
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

        public TheWallClock(ITestOutputHelper output) : base(output)
        {
        }
    }
}