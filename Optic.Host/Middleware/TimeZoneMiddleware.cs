namespace Optic.Host.Middleware;

public class TimeZoneMiddleware
{
    private readonly RequestDelegate _next;

    public TimeZoneMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var timeZoneId = context.Request.Headers["X-Time-Zone"].FirstOrDefault();

        TimeZoneInfo timeZone;

        try
        {
            timeZone = string.IsNullOrWhiteSpace(timeZoneId)
                ? TimeZoneInfo.Utc
                : TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            timeZone = TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            timeZone = TimeZoneInfo.Utc;
        }

        context.Items["TimeZone"] = timeZone;

        await _next(context);
    }
}