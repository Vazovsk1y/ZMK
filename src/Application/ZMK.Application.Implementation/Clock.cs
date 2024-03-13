using ZMK.Application.Services;

namespace ZMK.Application.Implementation;

public class Clock : TimeProvider, IClock
{
    public DateTimeOffset GetDateTimeOffsetUtcNow()
    {
        return GetUtcNow();
    }

    public DateTime GetDateTimeUtcNow()
    {
        return GetUtcNow().UtcDateTime;
    }
}