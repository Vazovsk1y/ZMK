namespace ZMK.Wpf.Constants;

public static class Cache
{
    public const string ProjectsCacheKey = "Projects";
    public static readonly TimeSpan ProjectsCacheExpiration = TimeSpan.FromMinutes(20);
}
