
namespace Optic.Host.Helpers;

public static class DateTimeHelper
{
    public static DateTime? ToRequestTime(DateTime? utcDate,HttpContext context)
    {
        if (utcDate == null)
            return null;

        var timeZone = context.Items["TimeZone"] as TimeZoneInfo ?? TimeZoneInfo.Utc;

        var utc = DateTime.SpecifyKind(utcDate.Value,DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone);
    }
}
