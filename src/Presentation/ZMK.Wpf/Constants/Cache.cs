namespace ZMK.Wpf.Constants;

public static class Cache
{
    public const string ProjectsPanelCacheKey = "Projects";
    public static readonly TimeSpan ProjectsPanelCacheExpiration = TimeSpan.FromMinutes(30);

    public const string EmployeesPanelCacheKey = "Employees";
    public static readonly TimeSpan EmployeesPanelCacheExpiration = TimeSpan.FromMinutes(30);

    public const string AreasPanelCacheKey = "Areas";
    public static readonly TimeSpan AreasPanelCacheExpiration = TimeSpan.FromHours(1);
}
