namespace ZMK.Application.Services;

public interface ICurrentSessionProvider
{
    Guid? GetCurrentSessionId();
}