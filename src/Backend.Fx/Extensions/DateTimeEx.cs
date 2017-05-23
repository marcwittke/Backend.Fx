namespace Backend.Fx.Extensions
{
    using System;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dayOfWeek"></param>
        /// <returns></returns>
        public static DateTime GetWeekDay(this DateTime dt, DayOfWeek dayOfWeek)
        {
            int diff = dt.DayOfWeek - dayOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(diff).Date;
        }

        public static long ToUnixEpochDate(this DateTime date)
        {
            return (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        }
    }
}
