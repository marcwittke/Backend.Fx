using System;
using System.Threading;
using Backend.Fx.Environment.DateAndTime;
using Xunit;

namespace Backend.Fx.Tests.Environment.DateAndTime
{
    public class TheFrozenClock
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
    }
}