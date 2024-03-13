namespace ZMK.Application.Services;

public interface IClock
{
    DateTimeOffset GetDateTimeOffsetUtcNow();

    DateTime GetDateTimeUtcNow();
}
