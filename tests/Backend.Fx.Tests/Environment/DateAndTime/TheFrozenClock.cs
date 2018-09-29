using Xunit;

namespace Backend.Fx.Tests.Environment.DateAndTime
{
    using System;
    using System.Threading;
    using Fx.Environment.DateAndTime;

    public class TheFrozenClock
    {
        [Fact]
        public void IsFrozen()
        {
            DateTime systemUtcNow = DateTime.UtcNow;
            IClock sut = FrozenClock.WithFrozenUtcNow(systemUtcNow);
            Assert.Equal(systemUtcNow, sut.UtcNow);
            Thread.Sleep(100);
            Assert.Equal(systemUtcNow, sut.UtcNow);
            Assert.NotEqual(DateTime.UtcNow, sut.UtcNow);
        }
    }
}