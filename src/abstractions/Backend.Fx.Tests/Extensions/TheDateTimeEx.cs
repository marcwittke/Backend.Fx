using System;
using Backend.Fx.Extensions;
using Xunit;

namespace Backend.Fx.Tests.Extensions
{
    public class TheDateTimeEx
    {
        [Fact]
        public void CanConvertToUnixEpochDate()
        {
            Assert.Equal(0L, new DateTime(1970, 1, 1).ToUnixEpochDate());
            Assert.Equal(1495675672L, new DateTime(2017, 5, 25, 1, 27, 52).ToUnixEpochDate());
            Assert.Equal(int.MaxValue, new DateTime(2038, 1, 19, 3, 14, 7).ToUnixEpochDate());
            Assert.Equal((long)int.MaxValue + 1, new DateTime(2038, 1, 19, 3, 14, 8).ToUnixEpochDate());
        }

        [Fact]
        public void CanGetStartOfWeek()
        {
            var dt = new DateTime(2017, 05, 24, 1, 2, 3);
            Assert.Equal(new DateTime(2017, 05, 22), dt.StartOfWeek());
            Assert.Equal(new DateTime(2017, 05, 21), dt.StartOfWeek(DayOfWeek.Sunday));
        }

        [Fact]
        public void CanGetWeekDay()
        {
            var dt = new DateTime(2017, 05, 24, 1, 2, 3);
            Assert.Equal(new DateTime(2017, 05, 22), dt.GetWeekDay(DayOfWeek.Monday));
            Assert.Equal(new DateTime(2017, 05, 23), dt.GetWeekDay(DayOfWeek.Tuesday));
            Assert.Equal(new DateTime(2017, 05, 24), dt.GetWeekDay(DayOfWeek.Wednesday));
            Assert.Equal(new DateTime(2017, 05, 25), dt.GetWeekDay(DayOfWeek.Thursday));
            Assert.Equal(new DateTime(2017, 05, 26), dt.GetWeekDay(DayOfWeek.Friday));
            Assert.Equal(new DateTime(2017, 05, 27), dt.GetWeekDay(DayOfWeek.Saturday));
            Assert.Equal(new DateTime(2017, 05, 28), dt.GetWeekDay(DayOfWeek.Sunday));
        }
    }
}
