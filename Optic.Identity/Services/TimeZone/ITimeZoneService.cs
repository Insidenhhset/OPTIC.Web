namespace Optic.Identity.Services.TimeZone;

public interface ITimeZoneService
{
    TimeZoneInfo GetCurrentTimeZone();

    DateTime ToLocalTime(DateTime utcDateTime);

    DateTime? ToLocalTime(DateTime? utcDateTime);
}