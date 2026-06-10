using System;
using System.Collections.Generic;
using System.Globalization;

namespace Utilities.Common.Extensions
{
    public static class DateTimeExt
    {
        public static List<DateTime> GetDateRange(this DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate) throw new ArgumentOutOfRangeException(nameof(endDate));
            List<DateTime> Result = new List<DateTime>();
            while (startDate <= endDate)
            {
                Result.Add(startDate);
                startDate = startDate.AddDays(1);
            }
            return Result;
        }

        public static int WeekOfYear(this DateTime date)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
        }

        public static DateTime FirstDateOfWeek(int Year, int WeekNumber, DayOfWeek dayOfWeek)
        {
            DateTime FirstDay = new DateTime(Year, 1, 1);
            while (FirstDay.DayOfWeek != dayOfWeek)
            {
                FirstDay = FirstDay.AddDays(1);
            }
            return (FirstDay.AddDays(7 * (WeekNumber - FirstDay.WeekOfYear())));
        }

        public static string ToStringWithTimeZone(this DateTime dateTime) => $"{dateTime:dd MMMM yyyy hh:mm:ss tt zzz}";

        [Obsolete("Obsolete")]
        public static DateTime GetWeekDay(this DateTime Date, DayOfWeek DayOfWeek)
        {
            return Date.AddDays(-(int)Date.DayOfWeek + (int)DayOfWeek);
        }
    }
}