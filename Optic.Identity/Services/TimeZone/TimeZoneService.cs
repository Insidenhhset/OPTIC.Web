namespace Optic.Identity.Services.TimeZone;

public class TimeZoneService : ITimeZoneService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TimeZoneService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public TimeZoneInfo GetCurrentTimeZone()
    {
        return _httpContextAccessor.HttpContext?.Items["TimeZone"] as TimeZoneInfo ?? TimeZoneInfo.Utc;
    }

    public DateTime ToLocalTime(DateTime utcDateTime)
    {
        var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc,GetCurrentTimeZone());
    }

    public DateTime? ToLocalTime(DateTime? utcDateTime)
    {
        return utcDateTime.HasValue ? ToLocalTime(utcDateTime.Value) : null;
    }
}