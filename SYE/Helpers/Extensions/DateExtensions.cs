using System;

namespace SYE.Helpers.Extensions
{
    public static class DateExtensions
    {
        public static DateTime GetLocalDateTime(this DateTime dateTime)
        {
            var BritishZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

            dateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Local, BritishZone);
        }
    }
}
