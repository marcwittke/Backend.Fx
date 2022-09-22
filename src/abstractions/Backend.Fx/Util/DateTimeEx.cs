using System;

namespace Backend.Fx.Util
{
    public static class DateTimeEx
    {
        /// <summary>
        /// Gets the related start of week of the DateTime at midnight.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="startOfWeek">Specify the DayOfWeek that you consider as first day of week. Default value: Monday</param>
        /// <returns></returns>
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime GetWeekDay(this DateTime dt, DayOfWeek dayOfWeek, DayOfWeek startOfWeek = DayOfWeek.Monday)
        {
            dt = dt.StartOfWeek(startOfWeek);
            while (dt.DayOfWeek != dayOfWeek)
            {
                dt = dt.AddDays(1);
            }

            return dt;
        }

        public static long ToUnixEpochDate(this DateTime utcDate)
        {
            return (long) Math.Round((utcDate - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds);
        }
    }
}