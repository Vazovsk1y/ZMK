using ZMK.Application.Services;

namespace ZMK.Wpf.Services;

public class CurrentSessionProvider : ICurrentSessionProvider
{
    public Guid? GetCurrentSessionId() => App.CurrentSession?.Id;
}